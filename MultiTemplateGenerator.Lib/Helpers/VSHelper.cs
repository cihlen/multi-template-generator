using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace MultiTemplateGenerator.Lib.Helpers
{
    public static class VSHelper
    {
        public static Version GetCurrentVSVersion()
        {
            var currProcess = Process.GetCurrentProcess();

            var vsProc = currProcess.ProcessName.Equals("devenv", StringComparison.InvariantCultureIgnoreCase)
                ? currProcess
                : Process.GetProcessesByName("devenv").FirstOrDefault();
            var vsFileName = vsProc?.MainModule?.FileName;

            if (string.IsNullOrWhiteSpace(vsFileName))
            {
                using var rkUninstall = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);
                foreach (var keyName in rkUninstall?.GetSubKeyNames() ?? new string[0])
                {
                    using var rk = rkUninstall?.OpenSubKey(keyName, false);
                    var displayIcon = (rk?.GetValue("DisplayIcon") as string)?.Trim('\"');
                    if (displayIcon?.EndsWith("devenv.exe", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        vsFileName = displayIcon;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(vsFileName))
                    return new Version(16, 8, 10000);
            }

            var fvi = FileVersionInfo.GetVersionInfo(vsFileName);
            return new Version(fvi.ProductVersion.Replace(',', '.'));
        }
    }
}
