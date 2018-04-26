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
using Exodus.Files;
using Exodus.HyperV;
using Exodus.IIS;

namespace Exodus
{
    public partial class Exodus : ServiceBase
    {
        // flags to determine whether or not the various processes have already begun
        bool b_WebConfigHasStarted = false;
        bool b_HyperVHasStarted = false;
        bool b_FilesHasStarted = false;

        // configure a timestamp of when the service started, to be appended as the backup directory path        
        public static string startTimeStamp; 

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
            // see here for configuring DateTime formats:  https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
            startTimeStamp = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");

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
            // Write this entry to the log just to verify that the service is actually doing something...
            //ExodusEventLog.WriteEntry("Exodus is monitoring the system...", EventLogEntryType.Information, 999);

            try
            {
                string xmlFile = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExodusConfig.xml"));
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlFile);

                // only start if the Web Config process has not yet been initiated
                if (!b_WebConfigHasStarted)
                {
                    try
                    {
                        b_WebConfigHasStarted = true;

                        string sitename = doc.SelectSingleNode("ExodusConfig/IIS/SiteName").InnerText;
                        //ExodusEventLog.WriteEntry("sitename: " + host, EventLogEntryType.Information, 178);
                        string siteaddress = doc.SelectSingleNode("ExodusConfig/IIS/SiteAddress").InnerText;
                        //ExodusEventLog.WriteEntry("siteaddress: " + host, EventLogEntryType.Information, 178);
                        string localpath = doc.SelectSingleNode("ExodusConfig/IIS/LocalPath").InnerText;
                        //ExodusEventLog.WriteEntry("localpath: " + backupdir, EventLogEntryType.Information, 178);

                        ExodusManager_IIS Manager_IIS = new ExodusManager_IIS();
                        Manager_IIS.AddSite(sitename, siteaddress, localpath);
                        Manager_IIS.AddApplicationPool("Exodus");
                    }
                    catch (Exception ex)
                    {
                        // write errors to the log
                        ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 177);
                    }
                }
                // only start if the Hyper-V backup process has not yet been initiated
                if (!b_HyperVHasStarted)
                {
                    try
                    {
                        b_HyperVHasStarted = true;

                        bool b_ExportAll = Convert.ToBoolean(doc.SelectSingleNode("ExodusConfig/HyperV/ExportAll").InnerText);
                        //ExodusEventLog.WriteEntry("b_ExportAll: " + b_ExportAll.ToString(), EventLogEntryType.Information, 178);
                        string host = doc.SelectSingleNode("ExodusConfig/HyperV/HostToQuery").InnerText;
                        //ExodusEventLog.WriteEntry("host: " + host, EventLogEntryType.Information, 178);
                        string backupdir = doc.SelectSingleNode("ExodusConfig/HyperV/BackupDestination").InnerText;
                        //ExodusEventLog.WriteEntry("backupdir: " + backupdir, EventLogEntryType.Information, 178);
                        int retain = Int32.Parse(doc.SelectSingleNode("ExodusConfig/HyperV/BackupsToRetain").InnerText);
                        //ExodusEventLog.WriteEntry("backups to retain: " + retain.ToString(), EventLogEntryType.Information, 178);
                        string user = doc.SelectSingleNode("ExodusConfig/HyperV/AdminUser").InnerText;
                        //ExodusEventLog.WriteEntry("user: " + user, EventLogEntryType.Information, 178);
                        string pass = doc.SelectSingleNode("ExodusConfig/HyperV/AdminPass").InnerText;
                        //ExodusEventLog.WriteEntry("pass: " + pass, EventLogEntryType.Information, 178);
                        string domain = doc.SelectSingleNode("ExodusConfig/HyperV/Domain").InnerText;
                        //ExodusEventLog.WriteEntry("domain: " + domain, EventLogEntryType.Information, 178);

                        if (b_ExportAll)
                        {
                            ExodusManager_HyperV Manager_HyperV = new ExodusManager_HyperV();
                            //Manager_HyperV.QueryInstance(host, "SELECT * FROM Msvm_ComputerSystem", user, pass, domain);
                            //Manager_HyperV.BackupHost(host, backupdir);
                            Manager_HyperV.DelegateAccount(host, backupdir);
                            Manager_HyperV.ManageBackupDirectory(backupdir, retain);
                            Manager_HyperV.GetAllVMs(host, backupdir);
                        }
                        else
                        {
                            ExodusEventLog.WriteEntry("Hyper-V Export Feature Disabled", EventLogEntryType.Warning, 130);
                        }
                    }
                    catch (Exception ex)
                    {
                        // write errors to the log
                        ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 177);
                    }
                }
                // only start if the Files backup process has not yet been initiated
                if (!b_FilesHasStarted)
                {
                    try
                    {
                        b_FilesHasStarted = true;

                        bool b_EnableMirror = Convert.ToBoolean(doc.SelectSingleNode("ExodusConfig/Files/EnableMirror").InnerText);
                        //ExodusEventLog.WriteEntry("b_EnableMirror: " + b_EnableMirror.ToString(), EventLogEntryType.Information, 178);
                        string source = doc.SelectSingleNode("ExodusConfig/Files/Source").InnerText;
                        //ExodusEventLog.WriteEntry("source: " + source, EventLogEntryType.Information, 178);
                        string destination = doc.SelectSingleNode("ExodusConfig/Files/Destination").InnerText;
                        //ExodusEventLog.WriteEntry("destination: " + destination, EventLogEntryType.Information, 178);

                        if (b_EnableMirror)
                        {
                            ExodusManager_Files Manager_Files = new ExodusManager_Files();
                            Manager_Files.MirrorFileShare(source, destination);
                        }
                        else
                        {
                            ExodusEventLog.WriteEntry("File Share Mirror Feature Disabled", EventLogEntryType.Warning, 130);
                        }
                    }
                    catch (Exception ex)
                    {
                        // write errors to the log
                        ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 177);
                    }
                }
            }
            catch (Exception ex)
            {
                // write errors to the log
                ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 177);
            }            
        }

        private void ExodusEventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
