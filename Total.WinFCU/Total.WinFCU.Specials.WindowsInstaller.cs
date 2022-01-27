using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Total.Util;

namespace Total.WinFCU
{
    public partial class fcu
    {
        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU clean the windows installer folder from obsolete files and folders
        //  How to determine what is right and what is wrong.
        //  - All valid packages are registered in "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-*\Products\*"
        //  - The InstallProperties key contains the LocalPackage property specifying the installerfile
        //  - The Patches key contains the AllPatches property specifying the key names of all applied patches
        //  - HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Patches\'patch-keyname' contains the
        //    LocalPackage name of the patch file
        //  All LocalPackages found in these hives are valid and will be compared with the actual ondisk files
        // --------------------------------------------------------------------------------------------------------------------
        public static void cleanWindowsInstaller()
        {
            // Specify the Windows Installer cache folder, and get the list of all packages and patches in it
            INF.filePath = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "Installer");
            List<string> packagesFound = new List<string>();
            packagesFound.AddRange(Directory.GetFiles(INF.filePath, "*.*", SearchOption.AllDirectories));

            // Get a list of all registered packages and patches (function returns null if anything went wrong)
            List<string> packagesRegistered = GetRegisteredPackages();
            if (packagesRegistered == null)
            {
                total.Logger.Warn("Something went wrong while searching for obsolete installer packages.");
                return;
            }

            // Compare packages found with packages registered. Those found but not registered end up on the obsoletePackages list
            // Skip the hidden patch cache though !!
            List<FileInfo> obsoletePackages = new List<FileInfo>();
            foreach (string packageFound in packagesFound)
            {
                if (packageFound.Contains("$PatchCache$")) { continue; }
                if (!packagesRegistered.Contains(packageFound, StringComparer.OrdinalIgnoreCase)) { obsoletePackages.Add(new FileInfo(packageFound)); }
            }
            total.Logger.Debug(String.Format("Found {0} obsolete files in the installer folder", obsoletePackages.Count));

            // zero the files in folder counters before deleting the files
            ZeroFolderCounters();
            fcu.DeleteFilesInList(obsoletePackages);

            // Once the files are removed, cleanup the empty folders anf again skip the hidden patch cache
            fcu.DeleteEmptyFolders(INF.filePath, new Regex(@".*\\\$PatchCache\$\\.*"), false, SearchOption.AllDirectories);
        }

        // This is a critical function, errors occurring in this function can result in the deletion of valid files!
        // So in case of whatever error -> quit and return null.
        public static List<string> GetRegisteredPackages()
        {
            // Define the 'root' of the Installer section in the HKLM hive
            const string userdataKeyName   = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData";
            // Create an empty list to store the registered packages
            List<string> registeredPackages = new List<string>();

            // Open the UserData registry key. Quit if none found (which should be impossible)
            RegistryKey userdataRegistryKey = Registry.LocalMachine.OpenSubKey(userdataKeyName);
            if (userdataRegistryKey == null)
            {
                total.Logger.Error("GetRegisteredPackages - no UserData registry key found!! - " + userdataKeyName);
                return null;
            }

            // Enumerate the (userSid) keynames in the UserData registry key
            foreach (string userSid in userdataRegistryKey.GetSubKeyNames())
            {
                // Use the userSid found to create the next registry keyname and open the key
                string userSidKeyName = userdataKeyName + "\\" + userSid;
                RegistryKey userSidRegistryKey = Registry.LocalMachine.OpenSubKey(userSidKeyName);
                if (userSidRegistryKey == null) { continue; }

                // Use the userSidRegistryKey to find and open the Products subkey. Also prepare the Patches subkey name
                string productsSubKeyName = userSidKeyName + "\\Products";
                string patchesSubKeyName  = userSidKeyName + "\\Patches";

                RegistryKey productsRegistryKey = Registry.LocalMachine.OpenSubKey(productsSubKeyName);
                if (productsRegistryKey == null) { continue; }
                total.Logger.Debug("GetRegisteredPackages - Registry path: " + productsSubKeyName);

                // Get all product codes from the Products key and get the install and patch details
                foreach (string ProductCode in productsRegistryKey.GetSubKeyNames())
                {
                    string productID = ConvertProductCodeToID(ProductCode);
                    string productsKeyName = productsSubKeyName + "\\" + ProductCode + "\\InstallProperties";
                    string patchesKeyName  = productsSubKeyName + "\\" + ProductCode + "\\Patches";
                    string localPackage = null;

                    // Get the local package name and add it to the list of registered packages. Skip if null or empty
                    RegistryKey productRegistryKey = Registry.LocalMachine.OpenSubKey(productsKeyName);
                    if (productRegistryKey == null) { continue; }
                    try
                    {
                        localPackage = (string)productRegistryKey.GetValue("LocalPackage");
                        string displayName = (string)productRegistryKey.GetValue("DisplayName");
                        if (String.IsNullOrEmpty(localPackage)) { continue; }
                        total.Logger.Debug(String.Format("GetRegisteredPackages -  ProductID: {0} [{1}]", productID, displayName));
                        registeredPackages.Add(localPackage);
                    }
                    catch (NullReferenceException) { continue; }
                    catch (Exception ex)
                    {
                        string errorMessage = String.Format("Error collecting product information for '{0}'", ProductCode);
                        total.Logger.Error(errorMessage, ex);
                        return null;
                    }

                    // Add the possible SourceHash file of this package and the possible files in the product subfolder
                    string localPackagePath = Path.GetDirectoryName(localPackage);
                    string subPackagePath = Path.Combine(localPackagePath, String.Format("{{{0}}}", productID));
                    string sourceHashFile = Path.Combine(localPackagePath, String.Format("SourceHash{{{0}}}", productID));
                    if (File.Exists(sourceHashFile)) { registeredPackages.Add(sourceHashFile); }
                    if (Directory.Exists(subPackagePath))
                    {
                        foreach (string productFile in Directory.GetFiles(subPackagePath, "*.*", SearchOption.AllDirectories))
                        {
                            if (File.Exists(productFile)) { registeredPackages.Add(productFile); }
                        }
                    }

                    // See whether there are applied patches to register
                    RegistryKey patchRegistryKey = Registry.LocalMachine.OpenSubKey(patchesKeyName);
                    if (patchRegistryKey == null) { continue; }

                    //  patchid's are stored as multi-string, use a dynamic variable to store it/them
                    dynamic allPatches = patchRegistryKey.GetValue("AllPatches");
                    foreach (string patchID in allPatches)
                    {
                        if (String.IsNullOrEmpty(patchID)) { continue; }
                        total.Logger.Debug(String.Format("GetRegisteredPatches  -    PatchID: {0}", patchID));
                        string patchSubKeyName = patchesSubKeyName + "\\" + patchID;
                        RegistryKey patchSubKey = Registry.LocalMachine.OpenSubKey(patchSubKeyName);
                        if (patchSubKey == null) { continue; }
                        try
                        {
                            localPackage = (string)patchSubKey.GetValue("LocalPackage");
                            if (!String.IsNullOrEmpty(localPackage)) { registeredPackages.Add(localPackage); }
                        }
                        catch (NullReferenceException) { continue; }
                        catch (Exception ex)
                        {
                            string errorMessage = String.Format("Error collecting patch details for '{0}'", patchID);
                            total.Logger.Error(errorMessage, ex);
                            return null;
                        }
                    }
                }
            }
            return registeredPackages;
        }

        public static string ConvertProductCodeToID(string ProductCode)
        {
            // ProductCode is a 32 character string which needs to be converted to a product ID
            // with thew following (GUID like) format: XXXXXXXX-YYYY-ZZZZ-RRRR-SSSSSSSSSSSS
            // Conversion: 08797B4A97059F548A7ECE028F34AD69 => A4B79780-5079-45F9-A8E7-EC20F843DA96
            if (ProductCode.Length != 32)
            {
                total.Logger.Error("ConvertProductCodeToID - input key must be 32 characters long");
                return null;
            }
            if (Encoding.UTF8.GetByteCount(ProductCode) != ProductCode.Length)
            {
                total.Logger.Error("ConvertProductCodeToID - only ASCII characters in input key");
                return null;
            }
            // 5 parts - build a new code string
            string productCode = null;
            // Part 1: 8 characters - reverse order (abcdefgh-> hgfedcba)
            for (int i = 0; i < 8; i++) { productCode += ProductCode[7 - i]; }
            // Part 2: 4 characters - reverse order (abcd -> dcba) (do not forget to insert the separator)
            productCode += "-";
            for (int i = 0; i < 4; i++) { productCode += ProductCode[11 - i]; }
            // Part 3: 4 characters - reverse order (abcd -> dcba)
            productCode += "-";
            for (int i = 0; i < 4; i++) { productCode += ProductCode[15 - i]; }
            // Part 4: 4 characters - reverse paired (abcd => badc)
            productCode += "-";
            productCode += String.Format("{0}{1}{2}{3}", ProductCode[17], ProductCode[16], ProductCode[19], ProductCode[18]);
            // Part 5:  12 characters - reverse paired
            productCode += "-";
            for (int i = 21; i < 32; i += 2) { productCode += String.Format("{0}{1}", ProductCode[i], ProductCode[i-1]); }
            // Thats it, return the result
            return productCode;
        }

    }
}
