<p align="center">
  <img src="logo\Total.png" alt="Total Productions"/>
</p>

# WinFCU
WinFCU is a rule based utility which keeps the file system clean by archiving/deleting/moving unwanted/unneeded files from the file system

WinFCU can be run interactivly, as a scheduled task or as a service

## Inno Setup

The WinFCU-x64 installer is created using "Inno Setup" v6.2.0

## Installing WinFCU

When installing WinFCU you have 3 options to choose from (Program Files are always installed. These are the WinFCU executable and require DLL Files);

- Configuration Files. These are the WinFCU.exe.config and the log4net.config
  Exclude these from installing when you are upgrading WinFCU (aka prevent loosing your current setup)
  Possible required changes to these files will be documented in the release notes
- Example Files. 3 example files can be installed in the WinFCU include folder. These files form a solid base of
  cleaning up the Temp, Windows and Users folders on the system drive
- Install WinFCU as service. This will do as it says......

### Commandline options

```
  .\WinFCU-x64-Setup_<version>.exe                          Installs everything and installs WinFCU as service
  .\WinFCU-x64-Setup_<version>.exe /COMPONENTS="configs"    Installs program files and core configuration files
  .\WinFCU-x64-Setup_<version>.exe /COMPONENTS="includes"   Installs program files and example include files
  .\WinFCU-x64-Setup_<version>.exe /COMPONENTS="service"    Installs program files and installs WinFCU as service
```

You can combine components in a comma separated string

```
  .\WinFCU-x64-Setup_<version>.exe /COMPONENTS="configs,service"  Will installs program files and core configuration
                                                                  files and finally install WinFCU as service
```

Other commandline options can be found at: <http://www.jrsoftware.org/ishelp/topic_setupcmdline.htm>

## Repo content

All mentioned files are part of the WinFCU installer and can be istalled as such

#### baseconfigs

2 base configuration files are supplied/needed;

- the WinFCU.exe.config which contains various default and runtime attributes, and also the cleanup directives for the WinFCU logs
- the log4net.config (if not provided, WinFCU will use hardcoded log4net settings)

#### includes

The includes folder contains 3 'example' configuration files for cleaning up specific parts of the Windows filesystem  
These files are 'ready-to-use' and can be installed by selecting 'Include Files" from the installer (or using /COMPONENTS="includes" on the command line)  

#### library

This folder contains the latest WinFCU images and the 3 DLL files used to build this version of WinFCU

#### logo

Contains the Total.Productions logo file in .png format  

#### installer

The WinFCU installer. Multiple installer versions can be found there including the Installer-Hash.txt file which contains the file hash values of the various versions

## Sources

WinFCU is open source, so its sources are also available via GitHub: https://github.com/navneev/WinFCU  

## License and Author

Author:: Hans van veen (hvveen@gmail.com)  

Copyright 2015-2019, Total Productions  

Licensed under the Apache License, Version 2.0 (the "License");  
you may not use this file except in compliance with the License.  
You may obtain a copy of the License at  

http://www.apache.org/licenses/LICENSE-2.0  
Unless required by applicable law or agreed to in writing, software  
distributed under the License is distributed on an "AS IS" BASIS,  
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  
See the License for the specific language governing permissions and  
limitations under the License.  
