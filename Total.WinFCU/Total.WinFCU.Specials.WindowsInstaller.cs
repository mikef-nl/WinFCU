using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace Total.WinFCU
{
    public partial class fcu
    {
        public static List<string> GetInstalledPackages()
        {
            List<string> validPackages = new List<string>();
            // Get all installed products (check the installed flag!!)
            IEnumerable<ProductInstallation> productInstallations = ProductInstallation.GetProducts(null, null, UserContexts.All);
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
            } 

            // Get all installed patches and remove superseded and obsolete patches
            IEnumerable<PatchInstallation>   allPatchInstallations = PatchInstallation.GetPatches(null, null, null, UserContexts.All, PatchStates.All);
            foreach (PatchInstallation patch in allPatchInstallations) { validPackages.Add(patch.LocalPackage); }
            // remove supperseded patches from list
            IEnumerable<PatchInstallation> supPatchInstallations = PatchInstallation.GetPatches(null, null, null, UserContexts.All, PatchStates.Superseded);
            foreach (PatchInstallation patch in supPatchInstallations) { if (validPackages.Contains(patch.LocalPackage)) { validPackages.Remove(patch.LocalPackage); } }
            // remove obsolete patches from list
            IEnumerable<PatchInstallation> obsPatchInstallations = PatchInstallation.GetPatches(null, null, null, UserContexts.All, PatchStates.Obsoleted);
            foreach (PatchInstallation patch in obsPatchInstallations) { if (validPackages.Contains(patch.LocalPackage)) { validPackages.Remove(patch.LocalPackage); } }

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
            return obsoleteFiles;
        }

    }
}
