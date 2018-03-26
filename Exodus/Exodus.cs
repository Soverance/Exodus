// © 2018 Soverance Studios
// Scott McCutchen
// soverance.com
// scott.mccutchen@soverance.com

using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using Exodus.HyperV;

namespace Exodus
{
    public partial class Exodus : ServiceBase
    {
        public Exodus()
        {
            InitializeComponent();
            ExodusEventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("Exodus Source"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "Exodus Source", "Exodus Event Log");
            }
            ExodusEventLog.Source = "Exodus Source";
            ExodusEventLog.Log = "Exodus Event Log";
        }

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // write a log entry to record service start
            ExodusEventLog.WriteEntry("The Exodus Data Management Service has started.", EventLogEntryType.Information, 01);

            // Set up a timer to trigger every minute.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds  
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            
            string xmlFile = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExodusConfig.xml"));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlFile);
            string host = doc.SelectSingleNode("HyperV/HostToQuery").InnerText;
            string user = doc.SelectSingleNode("HyperV/AdminUser").InnerText;
            string pass = doc.SelectSingleNode("HyperV/AdminPass").InnerText;
            string domain = doc.SelectSingleNode("HyperV/Domain").InnerText;

            ExodusManager_HyperV Manager_HyperV = new ExodusManager_HyperV();
            Manager_HyperV.QueryInstance(host, "SELECT * FROM Msvm_ComputerSystem", user, pass, domain);
        }

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // write a log entry to record service stop
            ExodusEventLog.WriteEntry("The Exodus Data Management Service has stopped.", EventLogEntryType.Information, 00);

            // Update the service state to Stopped.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.  
            ExodusEventLog.WriteEntry("Exodus is monitoring the system...", EventLogEntryType.Information, 99);
        }

        private void ExodusEventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
