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
        bool b_BackupHasStarted = false;

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
            ExodusEventLog.WriteEntry("The Exodus Data Management Service has started.", EventLogEntryType.Information, 101);

            // Set up a timer to trigger every minute.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds  
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }        

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // write a log entry to record service stop
            ExodusEventLog.WriteEntry("The Exodus Data Management Service has stopped.", EventLogEntryType.Information, 100);

            // Update the service state to Stopped.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        { 
            ExodusEventLog.WriteEntry("Exodus is monitoring the system...", EventLogEntryType.Information, 999);

            // only start if the backup process has not yet been initiated
            if (!b_BackupHasStarted)
            {
                try
                {
                    b_BackupHasStarted = true;

                    string xmlFile = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExodusConfig.xml"));
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlFile);

                    string host = doc.SelectSingleNode("ExodusConfig/HyperV/HostToQuery").InnerText;
                    //ExodusEventLog.WriteEntry("host: " + host, EventLogEntryType.Information, 178);
                    string backupdir = doc.SelectSingleNode("ExodusConfig/HyperV/BackupDestination").InnerText;
                    //ExodusEventLog.WriteEntry("host: " + host, EventLogEntryType.Information, 178);
                    int retain = Int32.Parse(doc.SelectSingleNode("ExodusConfig/HyperV/BackupsToRetain").InnerText);
                    //ExodusEventLog.WriteEntry("backups to retain: " + retain.ToString(), EventLogEntryType.Information, 178);
                    string user = doc.SelectSingleNode("ExodusConfig/HyperV/AdminUser").InnerText;
                    //ExodusEventLog.WriteEntry("user: " + user, EventLogEntryType.Information, 178);
                    string pass = doc.SelectSingleNode("ExodusConfig/HyperV/AdminPass").InnerText;
                    //ExodusEventLog.WriteEntry("pass: " + pass, EventLogEntryType.Information, 178);
                    string domain = doc.SelectSingleNode("ExodusConfig/HyperV/Domain").InnerText;
                    //ExodusEventLog.WriteEntry("domain: " + domain, EventLogEntryType.Information, 178);

                    ExodusManager_HyperV Manager_HyperV = new ExodusManager_HyperV();
                    //Manager_HyperV.QueryInstance(host, "SELECT * FROM Msvm_ComputerSystem", user, pass, domain);
                    //Manager_HyperV.BackupHost(host, backupdir);
                    Manager_HyperV.DelegateAccount(host, backupdir);
                    Manager_HyperV.ManageBackupDirectory(backupdir, retain);
                    Manager_HyperV.GetAllVMs(host, backupdir);                    
                }
                catch (Exception ex)
                {
                    // write errors to the log
                    ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 177);
                }
            }
        }

        private void ExodusEventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
