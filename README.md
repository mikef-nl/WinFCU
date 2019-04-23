<p align="center">
  <img src="files\logo\Total.png" alt="Total Productions"/>
</p>

# WinFCU
WinFCU is a rule based utility which keeps the file system clean by archiving/deleting/moving unwanted/unneeded files from the file system

WinFCU can be run interactivly, as a scheduled task or as a service

## Inno Setup

The WinFCU-x64-2.4.15.0.exe installer is created using "Inno Setup Compiler" v5.6.1(a)

## Installing WinFCU

When installing WinFCU you have 3 options to choose from (Program Files are always installed. These are the WinFCU executable and require DLL Files);

- Configuration Files. These are the WinFCU.exe.config and the log4net.config
  Exclude these from installing when you are upgrading WinFCU (aka prevent loosing your current setup)
  Possible required changes to these files will be documented in the release notes
- Example Files. 3 example files can be installed in the WinFCU folder. These files form a solid base of
  cleaning up the Tem, Windows and Users folders on the system drive
- Install WinFCU as service. This will do as it says......

###Commandline:

```
  .\WinFCU-x64-2.4.15.0.exe                          Installs everything and installs WinFCU as service
  .\WinFCU-x64-2.4.15.0.exe /COMPONENTS="configs"    Installs program files and core configuration files
  .\WinFCU-x64-2.4.15.0.exe /COMPONENTS="examples"   Installs program files and example include files
  .\WinFCU-x64-2.4.15.0.exe /COMPONENTS="service"    Installs program files and installs WinFCU as service
```

You can combine options in a comma separated string

```
  .\WinFCU-x64-2.4.15.0.exe /COMPONENTS="configs,service"  Will installs program files and core configuration
                                                           files and finally install WinFCU as service
```

Other commandline options can be found at: http://www.jrsoftware.org/ishelp/index.php?topic=runsection
