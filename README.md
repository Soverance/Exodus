# Exodus Data Management Service

Exodus Data Management Service by Soverance Studios helps manage data redundancy in hybrid on-prem + cloud environments. 
Efficiently process automatic redundant backups of Hyper-V infrastructure and file shares to both local and Azure storage.

## Configure ExodusConfig.xml
You must make a copy of the "ExodusConfigSample.xml" file, and rename it to "ExodusConfig.xml"

In the new ExodusConfig.xml file, modify the entries to reflect the configuration for your specific environment.

## Installing the Exodus Service
### Development builds: 
In an administrative session of the Visual Studio Developer Command Prompt, navigate to the ..\Exodus\bin\Debug folder and enter the following command:

```
installutil.exe Exodus.exe
```

### Release Builds:
Coming Soon...

## Starting and Stopping the Exodus Service
Once the service has been installed, you can simply use the "net start Exodus" or "net stop Exodus" commands to start and stop the service.  Alternatively, control the service via GUI with the "Services" MMC snap-in by typing "services.msc" into the Run command window.

## Uninstalling the Exodus Service
### For Development Builds:
In an administrative session of the Visual Studio Developer Command Prompt, navigate to the ..\Exodus\bin\Debug folder and enter the following command:

```
installutil.exe /u Exodus.exe
```

### Release Builds:
Coming Soon...

## Exodus Event Log Event IDs
* 00   - Exodus Service has stopped.
* 01   - Exodus Service has started.
* 99   - Exodus Service is monitoring the system.
* 200  - Hyper-V Management Query Machine Found (hostname)
* 201  - Hyper-V Management Query Exception Message