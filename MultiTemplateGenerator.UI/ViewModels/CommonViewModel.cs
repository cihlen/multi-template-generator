using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.UI.Helpers;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public abstract class CommonViewModel : ViewModelBase
    {
        protected virtual ILogger Logger { get; }

        protected CommonViewModel(ILogger logger)
        {
            Logger = logger;
        }

        protected virtual async Task SetErrorAsync(Exception exception, string message = null)
        {
            message ??= exception.Message;

            Logger.LogError(exception, message);
            await ShowErrorAsync(message);
        }

        protected virtual void SetError(Exception exception, string message = null)
        {
            message ??= exception.Message;

            Logger.LogError(exception, message);
            ShowError(message);
        }

        protected virtual void SetError(string message)
        {
            Logger.LogError(message);
            ShowError(message);
        }

        private bool _isMsgBoxOpen;

        protected async Task<MessageBoxResult> ShowMessageBoxAsync(string message, string title = null, MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage messageBoxImage = MessageBoxImage.Information)
        {
            if (_isMsgBoxOpen)
            {
                DialogHost.Close(ViewNames.MessageBoxDialogRoot, MessageBoxResult.Cancel);
            }

            var msgVm = new MessageBoxViewModel(message, title, buttons, messageBoxImage);
            _isMsgBoxOpen = true;
            try
            {
                await DialogHost.Show(msgVm, ViewNames.MessageBoxDialogRoot);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isMsgBoxOpen = false;
            }
            return msgVm.MessageBoxResult;
        }

        protected MessageBoxResult ShowMessageBox(string message, string title = null,
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage messageBoxImage = MessageBoxImage.Information)
        {
            var task = ShowMessageBoxAsync(message, title, buttons, messageBoxImage);
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected async Task ShowErrorAsync(string message, string title = null)
        {
            await ShowMessageBoxAsync(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected void ShowError(string message, string title = null)
        {
            message.ShowErrorMessageBox();
            //ShowMessageBoxAsync(message, title, MessageBoxButton.OK, MessageBoxImage.Error).Wait();
        }

        protected bool ShowQuestion(string message, string title = null, MessageBoxImage messageBoxImage = MessageBoxImage.Question)
        {
            return message.ShowQuestion(title, messageBoxImage);
        }

        #region Open/Execute

        private RelayCommand<string> _openLocationCommand;
        public RelayCommand<string> OpenLocationCommand => _openLocationCommand
            ??= new RelayCommand<string>(async (location) => await OpenLocationAsync(location), (location) => !IsInDesignMode /*&& location.DirectoryOrFileExists()*/);
        protected async Task OpenLocationAsync(string location)
        {
            try
            {
                if (IsInDesignMode)
                    return;

                if (!location.DirectoryOrFileExists())
                    return;

                ProcessHelper.OpenLocation(location);
            }
            catch (Exception e)
            {
                await SetErrorAsync(e, $"Error in OpenLocation: {location}");
            }
        }

        private RelayCommand<string> _executeFileCommand;

        public RelayCommand<string> ExecuteFileCommand => _executeFileCommand ??= new RelayCommand<string>(async (fileName) =>
            await ExecuteFileAsync(fileName), (filePath) => !IsInDesignMode && filePath.FileExists());

        protected async Task ExecuteFileAsync(string fileName)
        {
            try
            {
                if (!fileName.FileExists())
                {
                    $"{fileName} doesn't exist.".ShowWarning(AppHelper.ProductName);
                    return;
                }
                ProcessHelper.Start(fileName);
            }
            catch (Exception e)
            {
                await SetErrorAsync(e, "Error in ExecuteFile");
            }
        }

        private RelayCommand<string> _openInternetCommand;

        public RelayCommand<string> OpenInternetCommand => _openInternetCommand ??=
            new RelayCommand<string>(async (url) => await OpenInternet(url));

        protected async Task OpenInternet(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception e)
            {
                await SetErrorAsync(e);
            }
        }

        #endregion

        public string BrowseForLocation(string path, string defaultFolder = null)
        {
            if (IsInDesignMode)
                return "";

            Logger?.LogDebug($"BrowseForLocation({path})");

            var currentFolder = path;

            try
            {
                if (!string.IsNullOrEmpty(defaultFolder) && string.IsNullOrEmpty(currentFolder))
                    currentFolder = defaultFolder;

                return currentFolder.BrowseForFolder();
            }
            catch (Exception e)
            {
                SetError(e, $"Error in {nameof(BrowseForLocation)}: {path}");
                return null;
            }
        }

        public void BrowseForLocation(string path, string defaultFolder, Action<string> setAction)
        {
            if (IsInDesignMode)
                return;

            try
            {
                var newPath = BrowseForLocation(path, defaultFolder);
                if (!string.IsNullOrEmpty(newPath) && newPath != path)
                {
                    setAction.Invoke(newPath);
                }
            }
            catch (Exception e)
            {
                SetError(e, $"Error in {nameof(BrowseForLocation)}: {path} defaultFolder: {defaultFolder}");
            }
        }

        public string SelectFileFromSystem(string path, string defaultFolder, string title, string defaultExt, string filter)
        {
            if (IsInDesignMode)
                return "";

            Logger?.LogDebug($"SelectFileFromSystem({path})");

            try
            {
                var dlg = new OpenFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = defaultExt,
                    Filter = filter,
                    Title = title,
                    FileName = path,
                    InitialDirectory = defaultFolder,
                    Multiselect = false
                };

                return dlg.ShowDialog() == true ? dlg.FileName : null;
            }
            catch (Exception e)
            {
                SetError(e, $"Error selecting file: {path} defaultFolder: {defaultFolder}");
                return null;
            }
        }
    }
}
