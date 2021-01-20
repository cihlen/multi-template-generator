using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiTemplateGenerator.Lib;

namespace MultiTemplateGenerator.UI.Helpers
{
    public class ProcessStartupInfo
    {
        public string Filename { get; set; }

        public string Arguments { get; set; }

        public bool WaitForExit { get; set; }
    }

    public static class ProcessHelper
    {
        public static Process Execute(this ProcessStartupInfo processInfo, CancellationToken cancellation = default(CancellationToken), int waitInterval = 200)
        {
            using var p = new Process
            {
                StartInfo =
                {
                    FileName = processInfo.Filename,
                    Arguments = processInfo.Arguments
                }
            };

            if (!p.Start())
                return p;

            while (processInfo.WaitForExit && !p.HasExited)
            {
                cancellation.ThrowIfCancellationRequested();
                p.Refresh();
                p.WaitForInputIdle(waitInterval);
                Application.DoEvents();
            }
            return p;
        }

        public static Process Execute(string filename, string arguments = "", CancellationToken cancellation = default(CancellationToken), bool waitForExit = true,
                                      int waitInterval = 200)
        {
            return Execute(new ProcessStartupInfo { Filename = filename, Arguments = arguments, WaitForExit = waitForExit }, cancellation,
                waitInterval);
        }

        public static int StartWait(string filePath, string arguments = null, CancellationToken cancellation = default(CancellationToken), bool useShellExecute = true, bool noWindow = false, string verb = null)
        {
            return InternalStart(filePath, arguments, cancellation, true, useShellExecute, noWindow, verb);
        }

        public static int Start(string filePath, string arguments = null, bool useShellExecute = true, bool noWindow = false, string verb = null)
        {
            return InternalStart(filePath, arguments, CancellationToken.None, false, useShellExecute, noWindow, verb);
        }

        private static int InternalStart(string filePath, string arguments = null, CancellationToken cancellation = default(CancellationToken), bool wait = false, bool useShellExecute = true, bool noWindow = false, string verb = null)
        {
            //startInfo.Verb = "runas"; //Launch elevated
            ProcessStartInfo psi = new ProcessStartInfo
            {
                UseShellExecute = useShellExecute,
                FileName = filePath,
                Arguments = arguments ?? "",
                //Verb = verb
                CreateNoWindow = noWindow
            };

            if (!string.IsNullOrEmpty(verb))
                psi.Verb = verb;

            using (var p = Process.Start(psi))
            {
                if (p == null)
                    throw new Exception($"Error starting process. File={filePath} Arguments={arguments}");

                if (wait)
                {
                    //p.WaitForExit();

                    while (!p.HasExited)
                    {
                        if (cancellation != CancellationToken.None)
                            cancellation.ThrowIfCancellationRequested();
                        p.Refresh();
                        Task.Delay(500, cancellation).Wait(cancellation);
                        //p.WaitForInputIdle(200);
                        //Application.DoEvents();
                    }
                    return p.ExitCode;
                }
            }

            return 0;
        }

        public static void OpenLocation(string path)
        {
            if (!path.DirectoryOrFileExists() && path.GetDirectoryPath().DirectoryExists())
            {
                path = path.GetDirectoryPath();
            }
            if (path.DirectoryExists())
            {
                Process.Start(path);
                return;
            }

            string argument = "/select, \"" + path + "\"";
            Process.Start("explorer.exe", argument);
        }
    }
}
