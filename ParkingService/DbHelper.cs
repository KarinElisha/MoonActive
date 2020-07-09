using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ParkingService
{
    public static class DbHelper
    {
        private static string connetionString = "";

        static DbHelper()
        {
            string directory = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;

          //  string directory = Directory.GetCurrentDirectory();
            string dir = Directory.GetParent(Directory.GetParent(directory).FullName).FullName;
            string dbPath = Path.Combine(dir, "Database.mdf");
            connetionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True";
        }


        public static DataTable Select(string query)
        {
            SqlConnection conn = new SqlConnection(connetionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(query, conn);
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            conn.Close();
            return dt;
        }

        public static void Insert(string query)
        {
            SqlConnection conn;
            conn = new SqlConnection(connetionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
