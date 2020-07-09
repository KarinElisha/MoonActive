using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ParkingService
{
    public class Parking
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async System.Threading.Tasks.Task getData()
        {
            
            string directory = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;

            string[] ImagesPaths = Directory.GetFiles(Path.Combine(directory,"Images"));

            foreach (string imagePath in ImagesPaths)
            {
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = new TimeSpan(1, 1, 1);

                MultipartFormDataContent form = new MultipartFormDataContent();
                sendRequest(imagePath, httpClient, form);
                log.Info("sent request");

                HttpResponseMessage response = await httpClient.PostAsync("https://api.ocr.space/Parse/Image", form);
                log.Info("have a response");

                string strContent = await response.Content.ReadAsStringAsync();

                RootObject ocrResult = JsonConvert.DeserializeObject<RootObject>(strContent);
                string licensePlate = ocrResult.ParsedResults[0].ParsedText;
                editLicensePlate(ref licensePlate);

                bool isValid = isValidLicensePlateToPark(licensePlate, out string info);
                writeToDB(licensePlate, isValid, info);

                log.Info($"finished to check the image {imagePath}");
            }
        }

        private void sendRequest(string ImagePath, HttpClient httpClient, MultipartFormDataContent form)
        {
            httpClient.Timeout = new TimeSpan(1, 1, 1);
            form.Add(new StringContent("dc94155dbe88957"), "apikey"); //Added api key in form data
            form.Add(new StringContent("eng"), "language");

            byte[] imageData = File.ReadAllBytes(ImagePath);
            form.Add(new ByteArrayContent(imageData, 0, imageData.Length), "image", "image.jpg");
        }

        private bool isValidLicensePlateToPark(string licensePlate, out string info)
        {
            bool isValid = true;
            info = "";
            string[] arr = new string[6] { "85", "86", "87", "88", "89", "00" };
            List<string> endNumbers = new List<string>() { "85", "86", "87", "88", "89", "00" };
            string lastNumbers = licensePlate.Substring((licensePlate.Length - 2), 2);

            if (string.IsNullOrEmpty(licensePlate))
                return !isValid;
            if (licensePlate.EndsWith("25") || licensePlate.EndsWith("26"))
            {
                log.Info($"The {licensePlate} can't park");
                info = "Public transportation vehicles cannot enter the parking";
                return !isValid;
            }
            else if (Regex.IsMatch(licensePlate, @"[a-zA-Z]+"))
            {
                log.Info($"The {licensePlate} can't park");
                info = "Military and law enforcement vehicles are prohibited";
                return !isValid;
            }
            else if (endNumbers.Contains(lastNumbers))
            {
                log.Info($"The {licensePlate} can't park");
                info = "two last digits are 85/86/87/88/89/00";
                return !isValid;
            }
            else if (isDivid(licensePlate))
            {
                log.Info($"The {licensePlate} can't park");
                info = "suspected as operated by gas";
                return !isValid;
            }
            return isValid;
        }

        private bool isDivid(string licensePlate)
        {
            if (Int32.TryParse(licensePlate, out int numberLicence))
            {
                int sum = 0;
                while (numberLicence > 0)
                {
                    sum += numberLicence % 10;
                    numberLicence /= 10;
                }
                if (sum % 7 == 0)
                    return true;
            }
            return false;
        }

        private void writeToDB(string licensePlate, bool isValid, string info)
        {
            try
            {
                string query = $"INSERT INTO [dbo].[TableParking] (licensePlate,canPark,info) VALUES ('{licensePlate}','{isValid}','{info}')"; ;
                DbHelper.Insert(query);
                log.Info("Insert to DB");

            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private void checkDB()
        {
            string query = "select * from dbo.ParkingTable"; ;
            DataTable dataTable = DbHelper.Select(query);
        }

        private void editLicensePlate(ref string licensePlate)
        {
            licensePlate = licensePlate.Replace("-", "");
            licensePlate = licensePlate.Replace(":", "");

            char[] charsToTrim = { '\\', ' ', '\"', '\r', '\n' };
            licensePlate = licensePlate.Trim(charsToTrim);
            if (licensePlate.Length > 8)
            {
                licensePlate = licensePlate.Substring(0, 7);
            }
        }
    }
}
