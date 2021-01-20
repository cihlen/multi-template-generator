using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MultiTemplateGenerator.UI.Helpers
{
    public static class AppHelper
    {
        static AppHelper()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();
            ExecutablePath = assembly.Location; //Process.GetCurrentProcess().MainModule.FileName;
            ApplicationPath = Path.GetDirectoryName(ExecutablePath);
            ExecutableName = Path.GetFileName(ExecutablePath);
            
            var fvi = FileVersionInfo.GetVersionInfo(ExecutablePath);
            FileDescription = fvi.FileDescription;
            ProductName = fvi.ProductName;
            FileVersion = fvi.FileVersion;
            ProductVersion = fvi.ProductVersion;
        }

        public static string ProductVersion { get; }

        public static string FileVersion { get; }

        public static string ProductName { get; }

        public static string FileDescription { get; }

        public static string ExecutableName { get; }

        public static string ApplicationPath { get; }

        public static string ExecutablePath { get; }
    }
}
