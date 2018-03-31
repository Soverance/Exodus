# Exodus Data Management Service

Exodus Data Management Service by Soverance Studios helps manage data redundancy in hybrid on-prem + cloud environments. 
Efficiently process automatic redundant backups of Hyper-V infrastructure and file shares to both local and Azure storage.

## Prerequisites
* A Hyper-V host as a member of an Active Directory domain
* A server on which to install the Exodus service (it cannot be installed on the Hyper-V host)
* Microsoft .NET 4.6 or better
* Microsoft Hyper-V Module for Windows PowerShell
* Microsoft Active Directory Module for Windows PowerShell

## Configure Network Environment
* Enable WinRM on all machines that will interact with the Exodus service.  The preferred method is to do this via Group Policy within an Active Directory domain.
* If backing up to a network share, be sure to grant Full Control permissions over the share to the computer accounts which will interact with the share.

## Configure ExodusConfig.xml
You must make a copy of the "ExodusConfigSample.xml" file, and rename it to "ExodusConfig.xml"  In the new ExodusConfig.xml file, modify the entries to reflect the configuration for your specific environment.  

The "ExodusConfig.xml" file must be present with the installation before this service will run.  In the Solution Explorer, right-click the "ExodusConfig.xml" file and select "Properties", then set the value of the "Copy to Output Directory" attribute to "Copy Always".

## Installing the Exodus Service
### Development builds: 
In an administrative session of the Visual Studio Developer Command Prompt, navigate to the ..\Exodus\bin\Debug folder and enter the following command:

```
installutil.exe Exodus.exe
```

### Release Builds:
The Exodus service is not designed to be installed directly on a Hyper-V host.  Instead,  you must install the service on a remote computer (ideally a dedicated backup server). You can install the service by simply opening an administrative Powershell session, navigating to the "C:\Windows\Microsoft.NET\Framework\v4.0.30319\" directory, and then running the following command:

```
./installutil.exe "C:\Exodus\Exodus.exe"
```

where "C:\Exodus\Exodus.exe" is wherever you copied the release files to.

## Starting and Stopping the Exodus Service
Once the service has been installed, you can simply use the "net start Exodus" or "net stop Exodus" commands to start and stop the service.  Alternatively, control the service via GUI with the "Services" MMC snap-in by typing "services.msc" into the Run command window.

## Uninstalling the Exodus Service
### For Development Builds:
In an administrative session of the Visual Studio Developer Command Prompt, navigate to the ..\Exodus\bin\Debug folder and enter the following command:

```
installutil.exe /u Exodus.exe
```

### Release Builds:
You can uninstall the service on any computer by simply opening an administrative Powershell session, navigating to the "C:\Windows\Microsoft.NET\Framework\v4.0.30319\" directory, and then running the following command:

```
./installutil.exe /u "C:\Exodus\Exodus.exe"
```

where "C:\Exodus\Exodus.exe" is wherever you copied the release files to.

## Exodus Event Log Event IDs
These Event IDs will appear in the Event Viewer under the "Exodus Event Log".

* 100 - Exodus Service has stopped
* 101 - Exodus Service has started
* 177 - ExodusConfig.xml XML Load Error
* 178 - ExodusConfig.xml XML Value Output
* 200 - Hyper-V Management WQL Machine Found (hostname)
* 201 - Hyper-V PowerShell Backup Information
* 202 - Hyper-V Backup Directory Management Information
* 298 - Hyper-V PowerShell Error Stream
* 299 - Hyper-V Management Exception
* 624 - PowerShell VM Backup Incomplete
* 625 - PowerShell VM Backup Progress
* 626 - PowerShell VM Backup Complete
* 675 - Resource-Based Constrained Delegation Information
* 999 - Monitoring Notification