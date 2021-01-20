using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MultiTemplateGenerator.VS
{
    public static class MessageBoxExtensions
    {
        public static void ShowError(this AsyncPackage package, Exception exception, string title = null)
        {
            VsShellUtilities.ShowMessageBox(
                package,
                exception.ToString(),
                title,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
