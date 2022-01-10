using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Total.Util;

namespace Total.WinFCU
{
    public partial class fcu
    {
        public struct specialsInfo
        {
            public bool enabled;
            public bool systemOk;
            public string description;
            public string schedule;
        }
        public static Dictionary<string, specialsInfo> specialActions = new Dictionary<string, specialsInfo>();
        public static bool runHasSpecials = false;

        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU define Special Actions - define the list of hardcoded special actions
        // --------------------------------------------------------------------------------------------------------------------
        public static void defineSpecialActions()
        {
            // Declare the known 'Special Actions' - enabled and systemOk are initial always false!
            specialsInfo siObject = new specialsInfo();
            siObject.enabled = false;
            siObject.systemOk = false;
            siObject.schedule = "";
            // TestSpecialAction
            // siObject.description = "Test entry for Special Actions Items";
            // specialActions.Add("TestSpecialAction", siObject);
            // WindowsInstaller
            siObject.description = "Remove obsolete packages from the Windows Installer folder";
            specialActions.Add("WindowsInstaller", siObject);
            // RecycleBins
            // siObject.description = "Clear Recycle Bins on all drives";
            // specialActions.Add("RecycleBins", siObject);
        }

        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU show Special Actions - Show the list of hardcoded special actions
        // --------------------------------------------------------------------------------------------------------------------
        public static void showSpecialActions()
        {
            // Show the know specials
            Console.WriteLine("\r\n WinFCU Specials\r\n{0}", new string('-', 80));
            foreach (string spcName in specialActions.Keys)
            {
                specialsInfo siObject = specialActions[spcName];
                Console.WriteLine("  {0,-25} {1}", spcName, siObject.description);
            }
            Console.WriteLine("");
        }

        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU process Special Actions - process the list of selected hardcoded special actions
        // --------------------------------------------------------------------------------------------------------------------
        public static void processSpecialActions(string scheduleName)
        {
            // Processing specials require a schedule, they will NOT be targetted when the schedule requals #ALL# !!
            // This to prevent unnecessary clenup runs on potentially risky environments!
            foreach (string spcName in specialActions.Keys)
            {
                specialsInfo siObject = specialActions[spcName];
                total.Logger.Debug(String.Format("Verifying special: {0} ({1}/{2}/{3})", spcName, siObject.enabled, siObject.systemOk, siObject.schedule));
                if (!siObject.enabled) { continue; }
                if (scheduleName != "#ALL#" && scheduleName != siObject.schedule) { continue; }
                total.Logger.Info("Processing WinFCU Special: " + spcName);
                switch (spcName)
                {
                    case "WindowsInstaller": cleanWindowsInstaller(); break;
                    case "RecycleBins": cleanRecycleBins(); break;
                }

            }
        }

        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU clean the windows installer folder from obsolete files and folders
        // --------------------------------------------------------------------------------------------------------------------
        public static void cleanWindowsInstaller()
        {
            // Specify the standard Windows Installer cache folder
            INF.filePath = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "Installer");

            // Get the list of obsolete installer files and remove them from the system
            List<FileInfo> obsoleteFiles = getObsoleteWindowsInstallerFiles(INF.filePath);
            fcu.DeleteFilesInList(obsoleteFiles);

            // Once the files are removed, cleanup the empty folders
            fcu.DeleteEmptyFolders(INF.filePath, new Regex(@".*\\\$PatchCache\$\\.*"), false, SearchOption.AllDirectories);
        }

        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU clean the windows recycle bins
        // --------------------------------------------------------------------------------------------------------------------
        public static void cleanRecycleBins()
        {

        }
    }
}
