using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MultiTemplateGenerator.UI.Views;
using Task = System.Threading.Tasks.Task;

namespace MultiTemplateGenerator.VS
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenGeneratorCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("00ddf6c2-ba44-47e1-9980-fe1c9f035109");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenGeneratorCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenGeneratorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this._package = package ?? throw new ArgumentNullException(nameof(package));
            try
            {
                commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

                var menuCommandId = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.Execute, menuCommandId);
                commandService.AddCommand(menuItem);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                this._package.ShowError(e, "Error in OpenGeneratorCommand(params)");
                throw;
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenGeneratorCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            try
            {
                // Switch to the main thread - the call to AddCommand in OpenGeneratorCommand's constructor requires
                // the UI thread.
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

                OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                Instance = new OpenGeneratorCommand(package, commandService);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                package.ShowError(e, "Error in Execute");
                throw;
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var dte = (EnvDTE.DTE)ThreadHelper.JoinableTaskFactory.Run(() => ServiceProvider.GetServiceAsync(typeof(EnvDTE.DTE)));
                var solution = dte.Solution;

                IVsUIShell uiShell = (IVsUIShell)ThreadHelper.JoinableTaskFactory.Run(() => ServiceProvider.GetServiceAsync(typeof(IVsUIShell)));

                MultiTemplateView dialog = new MultiTemplateView(solution?.FullName);

                uiShell.GetDialogOwnerHwnd(out var hwnd);
                dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                uiShell.EnableModeless(0);
                try
                {
                    WindowHelper.ShowModal(dialog, hwnd);
                }
                finally
                {
                    // This will take place after the window is closed.  
                    uiShell.EnableModeless(1);
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
                _package.ShowError(exception, "Error in Execute");
                throw;
            }
        }
    }
}
