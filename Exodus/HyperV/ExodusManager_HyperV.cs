// © 2018 Soverance Studios
// Scott McCutchen
// soverance.com
// scott.mccutchen@soverance.com

using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Exodus.HyperV
{
    class ExodusManager_HyperV
    {
        public void QueryInstance(string computer, string query, string user, string pass, string domain)
        {
            try
            {
                Exodus ExodusRoot = new Exodus();
                
                ConnectionOptions connection = new ConnectionOptions();
                connection.Username = user;
                connection.Password = pass;
                connection.Authority = "ntlmdomain:" + domain;

                ManagementScope scope = new ManagementScope("\\\\" + computer + "\\root\\virtualization\\v2", connection);
                scope.Connect();

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                searcher.Scope = scope;

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    ExodusRoot.ExodusEventLog.WriteEntry(String.Format("Machine Found:  {0}\nStatus:  {1}\nDescription:  {2}", 
                        queryObj["ElementName"], queryObj["EnabledState"], queryObj["Description"]), EventLogEntryType.Information, 200);
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 299);
            }
        }

        public void DelegateAccount(string host, string path)
        {
            try
            {
                Exodus ExodusRoot = new Exodus();

                // create a new PS instance
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                    // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                    // use "AddParameter" to add a single parameter to the last command/script on the pipeline.

                    PowerShellInstance.AddScript(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HyperV/DelegateAccount.ps1"
                        + " -computer " + host + " -path " + path));

                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    foreach (PSObject outputItem in PSOutput)
                    {
                        if (outputItem != null)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry(outputItem.BaseObject.GetType().FullName + "\n" + outputItem.BaseObject.ToString(), EventLogEntryType.Information, 201);
                        }
                    }

                    if (PowerShellInstance.Streams.Error.Count > 0)
                    {
                        foreach (ErrorRecord error in PowerShellInstance.Streams.Error)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry("Error Details:  " + error.ErrorDetails + "\n" +
                                "Exception:  " + error.Exception, EventLogEntryType.Error, 298);
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 299);
            }
        }

        public void ManageBackupDirectory(string path, int retain)
        {
            try
            {
                Exodus ExodusRoot = new Exodus();

                // configure a dated directory path
                string datepath = path + Exodus.startTimeStamp;

                // create the directory for today's backup
                // this method does nothing if the directory already exists.
                System.IO.Directory.CreateDirectory(datepath);
                string message0 = "The backup directory " + datepath + " was created.";
                ExodusRoot.ExodusEventLog.WriteEntry(message0, EventLogEntryType.Information, 201);

                // if there are more backup directories than specified
                if (System.IO.Directory.GetDirectories(@path).Length > retain)
                {
                    string message1 = System.IO.Directory.GetDirectories(@path).Length.ToString() + " directories were found, which is greater than the retention count specified in ExodusConfig.xml.  Deleting oldest backup directory.";
                    ExodusRoot.ExodusEventLog.WriteEntry(message1, EventLogEntryType.Information, 201);

                    // this code gets all the directories in the specified remote path, orders them youngest first, skips the specified retain count, and deletes everything else
                    foreach (var fi in new DirectoryInfo(path).GetDirectories().OrderByDescending(x => x.LastWriteTime).Skip(retain))
                    {
                        fi.Delete(true);  // recursively delete the directory
                        string message2 = fi.FullName + " was deleted.";
                        ExodusRoot.ExodusEventLog.WriteEntry(message2, EventLogEntryType.Information, 201);
                    }
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 299);
            }            
        }

        public void GetAllVMs(string host, string path)
        {
            try
            {
                Exodus ExodusRoot = new Exodus();

                // create a new PS instance
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                    // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                    // use "AddParameter" to add a single parameter to the last command/script on the pipeline.

                    PowerShellInstance.AddCommand("Get-VM");                    
                    PowerShellInstance.AddParameter("ComputerName", host);

                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    foreach (PSObject outputItem in PSOutput)
                    {
                        if (outputItem != null)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry(outputItem.BaseObject.GetType().FullName
                                + "\n" + outputItem.BaseObject.ToString(), EventLogEntryType.Information, 201);

                            string vm = outputItem.Members["Name"].Value.ToString();
                            BackupVM(host, vm, path);
                        }
                    }

                    if (PowerShellInstance.Streams.Error.Count > 0)
                    {
                        foreach (ErrorRecord error in PowerShellInstance.Streams.Error)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry("Error Details:  " + error.ErrorDetails + "\n" +
                                "Exception:  " + error.Exception, EventLogEntryType.Error, 298);
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 299);
            }
        }

        public void BackupVM(string host, string vm, string path)
        {
            try
            {
                Exodus ExodusRoot = new Exodus();

                // configure a dated directory path
                string datepath = path + Exodus.startTimeStamp;

                // create a new PS instance
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                    // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                    // use "AddParameter" to add a single parameter to the last command/script on the pipeline.

                    PowerShellInstance.AddScript(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HyperV/BackupOne.ps1" 
                        + " -hypervhost " + host + " -vm " + vm + " -path " + datepath));

                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    foreach (PSObject outputItem in PSOutput)
                    {
                        if (outputItem != null)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry(outputItem.BaseObject.GetType().FullName + "\n" + outputItem.BaseObject.ToString(), EventLogEntryType.Information, 201);
                        }
                    }

                    if (PowerShellInstance.Streams.Error.Count > 0)
                    {
                        foreach (ErrorRecord error in PowerShellInstance.Streams.Error)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry("Error Details:  " + error.ErrorDetails + "\n" +
                                "Exception:  " + error.Exception, EventLogEntryType.Error, 298);
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 299);
            }
        }

        public void BackupAll(string hostname, string backupdir)
        {
            try
            {
                Exodus ExodusRoot = new Exodus();
                
                // create a new PS instance
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                    // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                    // use "AddParameter" to add a single parameter to the last command/script on the pipeline.

                    PowerShellInstance.AddScript(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HyperV/BackupAll.ps1" 
                        + " -hypervhost " + hostname + " -path " + backupdir));

                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    foreach (PSObject outputItem in PSOutput)
                    {
                        if (outputItem != null)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry(outputItem.BaseObject.GetType().FullName + "\n" + outputItem.BaseObject.ToString(), EventLogEntryType.Information, 201);
                        }
                    }

                    if (PowerShellInstance.Streams.Error.Count > 0)
                    {
                        foreach (ErrorRecord error in PowerShellInstance.Streams.Error)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry("Error Details:  " + error.ErrorDetails + "\n" +
                                "Exception:  " + error.Exception, EventLogEntryType.Error, 298);
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 299);
            }
            
        }
    }
}
