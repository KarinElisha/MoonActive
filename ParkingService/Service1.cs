using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ParkingService
{
    public partial class Service1 : ServiceBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                log.Info("application started");
                Parking parking = new Parking();
                parking.getData().Wait();
                log.Info("finished to overall the license plates");
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        protected override void OnStop()
        {
        }
    }
}
