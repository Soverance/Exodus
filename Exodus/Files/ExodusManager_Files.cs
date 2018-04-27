// © 2018 Soverance Studios
// Scott McCutchen
// soverance.com
// scott.mccutchen@soverance.com

using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Management;
using System.Management.Automation;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Exodus.Files
{
    class ExodusManager_Files
    {
        public void MirrorFileShare(string source, string destination)
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

                    PowerShellInstance.AddScript(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/RoboCopyMirror.ps1"
                        + " -source " + source + " -destination " + destination));

                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    string RoboCopyOutput = "";

                    foreach (PSObject outputItem in PSOutput)
                    {
                        if (outputItem != null)
                        {
                            RoboCopyOutput += outputItem.BaseObject.ToString() + System.Environment.NewLine;
                            //ExodusRoot.ExodusEventLog.WriteEntry(outputItem.BaseObject.GetType().FullName + "\n" + outputItem.BaseObject.ToString(), EventLogEntryType.Information, 301);
                        }
                    }

                    if (RoboCopyOutput != null)
                    {
                        ExodusRoot.ExodusEventLog.WriteEntry(RoboCopyOutput, EventLogEntryType.Information, 302);
                    }

                    if (PowerShellInstance.Streams.Error.Count > 0)
                    {
                        foreach (ErrorRecord error in PowerShellInstance.Streams.Error)
                        {
                            ExodusRoot.ExodusEventLog.WriteEntry("Error Details:  " + error.ErrorDetails + "\n" +
                                "Exception:  " + error.Exception, EventLogEntryType.Error, 398);
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 399);
            }
        }

        public static void ModifyDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account,
            Rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
            ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }
    }
}
