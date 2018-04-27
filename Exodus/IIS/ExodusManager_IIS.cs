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
using Microsoft.Web.Administration;

namespace Exodus.IIS
{
    class ExodusManager_IIS
    {
        Exodus ExodusRoot = new Exodus();

        // Creates a new IIS website to serve the Exodus Web API
        public void AddSite(string SiteName, string SiteAddress, string LocalPath)
        {
            try
            {
                ServerManager serverManager = new ServerManager();  // new IIS Server Manager object

                // Make sure there is at least one site in the IIS Server, probably the IIS "Default Web Site"
                if (serverManager.Sites != null && serverManager.Sites.Count > 0)
                {
                    // check to make sure the Exodus site does not already exist
                    if (serverManager.Sites.FirstOrDefault(s => s.Name == SiteName) == null)
                    {
                        string ip = "*";
                        string port = "80";
                        string hostName = SiteAddress;
                        string bindingInfo = string.Format(@"{0}:{1}:{2}", ip, port, hostName);

                        Site exodusSite = serverManager.Sites.Add(SiteName, "http", bindingInfo, LocalPath);  // reference to newly created site
                        exodusSite.ServerAutoStart = true;  // enable auto-start for the new site
                        serverManager.CommitChanges();  // commit changes to IIS

                        ExodusRoot.ExodusEventLog.WriteEntry("A new IIS site named " + SiteName + " was created.", EventLogEntryType.Information, 140);
                    }
                    else
                    {
                        // write errors to the log
                        ExodusRoot.ExodusEventLog.WriteEntry("The " + SiteName + " web site already exists in IIS.", EventLogEntryType.Warning, 148);
                    }
                }
                else
                {
                    // write errors to the log
                    ExodusRoot.ExodusEventLog.WriteEntry("At least one site must always exist in IIS. ", EventLogEntryType.Error, 149);
                }
            }
            catch (Exception ex)
            {
                // write errors to the log
                ExodusRoot.ExodusEventLog.WriteEntry("IIS WEB SITE ERROR: " + ex.Message, EventLogEntryType.Error, 149);
            }
            
        }

        // Creates a new application pool to serve the IIS website
        public void AddApplicationPool(string SiteName)
        {
            try
            {
                ServerManager serverManager = new ServerManager();  // new IIS Server Manager object

                // check to ensure the IIS server has at least one application pool, probably the "DefaultAppPool"
                if (serverManager.ApplicationPools != null && serverManager.ApplicationPools.Count > 0)
                {
                    // check to make sure the Exodus application pool does not yet exist
                    if (serverManager.ApplicationPools.FirstOrDefault(p => p.Name == SiteName) == null)
                    {
                        serverManager.ApplicationPools.Add(SiteName);  // create new app pool
                        serverManager.Sites[SiteName].Applications[0].ApplicationPoolName = SiteName;  // configure Exodus site to use the new app pool
                        ApplicationPool AppPool = serverManager.ApplicationPools[SiteName];  // new IIS App Pool object
                        AppPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                        serverManager.CommitChanges();  // commit changes to IIS
                        AppPool.Recycle();  // recycle the app pool

                        ExodusRoot.ExodusEventLog.WriteEntry("The Exodus application pool was created.", EventLogEntryType.Information, 140);
                    }
                    else
                    {
                        ApplicationPool AppPool = serverManager.ApplicationPools[SiteName];  // new IIS App Pool object
                        AppPool.Recycle();  // recycle the app pool
                        // write errors to the log
                        ExodusRoot.ExodusEventLog.WriteEntry("The " + SiteName + " application pool already exists in IIS, and was recycled.", EventLogEntryType.Warning, 148);
                    }
                }
                else
                {
                    // write errors to the log
                    ExodusRoot.ExodusEventLog.WriteEntry("At least one application pool must always exist in IIS.", EventLogEntryType.Error, 149);
                }                
            }
            catch (Exception ex)
            {
                // write errors to the log
                ExodusRoot.ExodusEventLog.WriteEntry("APPLICATION POOL ERROR: " + ex.Message, EventLogEntryType.Error, 149);
            }            
        }
    }
}
