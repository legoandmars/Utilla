using System;
using System.Runtime.InteropServices;

namespace Utilla.Utils
{
    public static class PathUtils
    {
        private const string DownloadsFolderGuid = @"{374DE290-123F-4565-9164-39C4925E467B}";
        private const uint FolderFlagDontVerify = 0x00004000;

        public static string GetDownloadsFolderPath()
        {
            var result = SHGetKnownFolderPath(new Guid(DownloadsFolderGuid), FolderFlagDontVerify, new IntPtr(0), out var outPath);
            if (result >= 0)
            {
                var path = Marshal.PtrToStringUni(outPath);
                Marshal.FreeCoTaskMem(outPath);
                return path;
            }
            else
            {
                throw new ExternalException("Unable to retrieve the known folder path. It may not be available on this system.", result);
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
    }
}