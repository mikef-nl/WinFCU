using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;
using System.Text.RegularExpressions;
using Total.Util;


namespace Total.WinFCU
{
    public partial class fcu
    {
        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU clean the windows installer folder from obsolete files and folders
        // --------------------------------------------------------------------------------------------------------------------
        public static void cleanWindowsInstaller()
        {
            // Specify the standard Windows Installer cache folder
            INF.filePath = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "Installer");

            // Get the list of obsolete installer files and remove them from the system
            List<FileInfo> obsoleteFiles = getObsoleteWindowsInstallerFiles(INF.filePath);
            // zero counters before deleting the files
            ZeroFolderCounters();
            fcu.DeleteFilesInList(obsoleteFiles);

            // Once the files are removed, cleanup the empty folders
            fcu.DeleteEmptyFolders(INF.filePath, new Regex(@".*\\\$PatchCache\$\\.*"), false, SearchOption.AllDirectories);
        }

        public static List<string> GetInstalledPackages()
        {
            List<string> validPackages = new List<string>();
            // Get all installed products (check the installed flag!!)
            IEnumerable<ProductInstallation> productInstallations = null;
            try { productInstallations = ProductInstallation.GetProducts(null, null, UserContexts.All); }
            catch (Exception ex) { total.Logger.Error("Error collecting installed package information.\n", ex); Environment.Exit(1); }

            foreach (ProductInstallation product in productInstallations)
            {
                if (!product.IsInstalled) { continue; }
                validPackages.Add(product.LocalPackage);
                if (product.ProductName.Contains("Click-to-Run")) { continue; }
                string localPackagePath = Path.GetDirectoryName(product.LocalPackage);
                // Check for files in possible product sub folders
                string subPackagePath   = Path.Combine(localPackagePath, product.ProductCode);
                if (Directory.Exists(subPackagePath))
                {
                    foreach (string installerFile in Directory.GetFiles(subPackagePath, "*.*", SearchOption.AllDirectories))
                    {
                        if (!string.IsNullOrEmpty(installerFile)) { validPackages.Add(installerFile); }
                    }
                }
                // Check for possible SourceHash files
                string sourceHashFile   = String.Format("SourceHash{0}", product.ProductCode);
                if (File.Exists(sourceHashFile)) { validPackages.Add(Path.Combine(localPackagePath, sourceHashFile)); }
                // Check for applied patches and add them to the list
                try
                {
                    IEnumerable<PatchInstallation> patches = PatchInstallation.GetPatches(null, product.ProductCode, null, UserContexts.All, PatchStates.All);
                    foreach (PatchInstallation patch in patches) { validPackages.Add(patch.LocalPackage); }
                }
                catch (Exception ex) {
                    string errorMessage = String.Format("Error collecting patch information for '{0}' [{1}]\n", product.ProductName, product.ProductCode);
                    total.Logger.Error(errorMessage, ex);
                    Environment.Exit(1);
                }
            }

            IEnumerable<PatchInstallation> productPatches = null;
            // remove supperseded patches from list
            try { productPatches = PatchInstallation.GetPatches(null, null, null, UserContexts.All, PatchStates.Superseded); }
            catch (Exception ex) { total.Logger.Error("Error collecting superseded patch information.\n", ex); Environment.Exit(1); }
            foreach (PatchInstallation patch in productPatches) { if (validPackages.Contains(patch.LocalPackage)) { validPackages.Remove(patch.LocalPackage); } }

            // remove obsolete patches from list
            try { productPatches = PatchInstallation.GetPatches(null, null, null, UserContexts.All, PatchStates.Obsoleted); }
            catch (Exception ex) { total.Logger.Error("Error collecting obsoleted patch information.\n", ex); Environment.Exit(1); }
            foreach (PatchInstallation patch in productPatches) { if (validPackages.Contains(patch.LocalPackage)) { validPackages.Remove(patch.LocalPackage); } }

            // Return the resulting list
            return validPackages;
        }

        public static List<FileInfo> getObsoleteWindowsInstallerFiles(string installerDirectory)
        {
            // Create list with valid (aka active) package and patch files
            List<string> validFiles = GetInstalledPackages();

            // Create a list of obsolete files by comparing the actual folder content with the list of valid files
            List<FileInfo> obsoleteFiles = new List<FileInfo>();
            string[] installerFiles = Directory.GetFiles(installerDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string installerFile in installerFiles)
            {
                if (installerFile.Contains("$PatchCache$")) { continue; }
                if (!validFiles.Contains(installerFile, StringComparer.OrdinalIgnoreCase)) { obsoleteFiles.Add(new FileInfo(installerFile)); }
            }

            // Return the list of obsolete installer files
            total.Logger.Debug(String.Format("Found {0} obsolete files in the installer folder", obsoleteFiles.Count));
            return obsoleteFiles;
        }

    }
}
