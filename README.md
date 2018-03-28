# Exodus Data Management Service

Exodus Data Management Service by Soverance Studios helps manage data redundancy in hybrid on-prem + cloud environments. 
Efficiently process automatic redundant backups of Hyper-V infrastructure and file shares to both local and Azure storage.

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
* 999 - Exodus Service is monitoring the system
* 177 - ExodusConfig.xml XML Load Error
* 200 - Hyper-V Management Query Machine Found (hostname)
* 201 - Hyper-V Management Query Exception Message