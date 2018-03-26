// © 2018 Soverance Studios
// Scott McCutchen
// soverance.com
// scott.mccutchen@soverance.com

using System;
using System.Management;
using System.Diagnostics;

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
                connection.Authority = domain;

                ManagementScope scope = new ManagementScope("\\\\" + computer + "\\root\\virtualization\\v2", connection);
                scope.Connect();

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                searcher.Scope = scope;

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    ExodusRoot.ExodusEventLog.WriteEntry(String.Format("Machine Found:  {0}", queryObj["ElementName"]), EventLogEntryType.Information, 200);
                    
                }
            }
            catch (ManagementException ex)
            {
                // Handle the exception as appropriate.
                Exodus ExodusRoot = new Exodus();
                ExodusRoot.ExodusEventLog.WriteEntry(ex.Message, EventLogEntryType.Information, 201);
            }
        }
    }
}
