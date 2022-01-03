using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPEGgerServer
{
    public partial class Service1 : ServiceBase
    {
        Thread listenerThread;
        TCPListener tCPListener;

        public Service1()
        {
            InitializeComponent();
            // ConnectSocket();
        }

        protected override void OnStart(string[] args)
        {
            Logger.LogWithNoLock(" Service Started ");

            Logger.LogWithNoLock($" -------- Maximum file size for the log is {Logger.logsize}MB --------");
            listenerThread = new Thread(this.ConnectSocket);
            listenerThread.Start();
        }

        protected override void OnStop()
        {
            tCPListener.StopListener();
            listenerThread.Abort();
            Logger.LogWithNoLock(" Service stopped ");
        }

        private void ConnectSocket()
        {
            try
            {
                Logger.LogWithNoLock(" Service Started ");
                Logger.LogWithNoLock($" -------- Maximum file size for the log is {Logger.logsize}MB --------");
                tCPListener = new TCPListener();
                tCPListener.StartListener();
            }
            catch (Exception ex)
            {
                Logger.LogWithNoLock($" Exception at ConnectSocket :{ex.Message} ");
            }
        }
    }
}
