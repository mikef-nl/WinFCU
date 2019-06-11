<p align="center">
  <img src="logo\Total.png" alt="Total Productions"/>
</p>

# WinFCU
WinFCU is a rule based utility which keeps the file system clean by archiving/deleting/moving unwanted/unneeded files from the file system

WinFCU can be run interactivly, as a scheduled task or as a service

## V2.4.15.0

Replacement keyword handling has been rewritten. Purpose is to create auto-documentation of the keywords.  
WinFCU.exe now has a '-show keywords' option which will show all default, application and user defined keywords including a description and value example per keyword.

- Changes to WinFCU.exe.config  
  `<includeFiles>` is now a multi element XML node (aka multiple `<path=.... />` elements can be provided)  
  `<keyWords>` elements now have next to the key and value attributes a description attribute (see 'purpose' above)  

## 2.4.16.0

Fixed a bug with reloading schedules  
Added some example data to the WinFCU.exe.config file

## 2.4.16.1

Fixed '-show keywords' output formatting  
Stripped version from installer filename  
Fixed LogFiles folder spec in example file

## 2.4.16.2

Clear total counts before a run starts  
