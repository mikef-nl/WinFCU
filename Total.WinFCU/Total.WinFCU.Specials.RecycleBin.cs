using System;
using System.Runtime.InteropServices;
using Total.Util;

namespace Total.WinFCU
{
    public partial class fcu
    {
        // Structure used by SHQueryRecycleBin.
        [StructLayout(LayoutKind.Sequential)]
        private struct SHQUERYRBINFO
        {
            public int  cbSize;
            public long i64Size;
            public long i64NumItems;
        }

        [DllImport("shell32.dll")]
        private static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

        [DllImport("shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hWnd, string pszRootPath, uint dwFlags);

        // --------------------------------------------------------------------------------------------------------------------
        //  WinFCU clean the windows recycle bins
        // --------------------------------------------------------------------------------------------------------------------
        public static void cleanRecycleBins()
        {
            // Do not show confirm message box
            const int SHERB_NOCONFIRMATION = 0x00000001;

            SHQUERYRBINFO sqrbi = new SHQUERYRBINFO();
            sqrbi.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
            int hresult = SHQueryRecycleBin(string.Empty, ref sqrbi);

            if (sqrbi.i64NumItems == 0)
            {
                total.Logger.Debug("No files found in recycle bins");
                return;
            }
            if (sqrbi.i64NumItems == 1) { total.Logger.Info(String.Format("Found 1 deleted file in the recyclebin ({0} bytes)", sqrbi.i64Size)); }
            else { total.Logger.Info(String.Format("Found {0} deleted files in the recyclebin ({1} bytes)", sqrbi.i64NumItems, sqrbi.i64Size)); }

            // empty Recycle Bin and add the number of bytes deleted tot the total ammount
            if (!total.APP.Dryrun) {
                SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION);
                total_bytesDeleted += sqrbi.i64Size;
            }
        }

    }
}
