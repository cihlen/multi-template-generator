using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.UI.Helpers;
using Serilog;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public abstract class CommonViewModel : ViewModelBase
    {
        protected virtual ILogger Logger { get; }

        protected CommonViewModel(ILogger logger)
        {
            Logger = logger;
        }

        protected virtual void SetError(Exception exception, string message = null)
        {
            if (message == null)
                message = exception.Message;

            Logger.Error(exception, message);
            message.ShowErrorMessageBox();
        }

        protected virtual void SetError(string message)
        {
            Logger.Error(message);
            message.ShowErrorMessageBox();
        }

        #region Open/Execute

        private RelayCommand<string> _openLocationCommand;
        public RelayCommand<string> OpenLocationCommand => _openLocationCommand
            ??= new RelayCommand<string>(OpenLocation, (location) => !IsInDesignMode /*&& location.DirectoryOrFileExists()*/);
        protected void OpenLocation(string location)
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
                SetError(e, $"Error in OpenLocation: {location}");
            }
        }

        private RelayCommand<string> _executeFileCommand;

        public RelayCommand<string> ExecuteFileCommand => _executeFileCommand ??= new RelayCommand<string>(ExecuteFile, (filePath) => !IsInDesignMode && filePath.FileExists());

        protected void ExecuteFile(string fileName)
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
                SetError(e, "Error in ExecuteFile");
            }
        }

        private RelayCommand<string> _openInternetCommand;

        public RelayCommand<string> OpenInternetCommand => _openInternetCommand ??= new RelayCommand<string>(OpenInternet);

        protected void OpenInternet(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception e)
            {
                SetError(e);
            }
        }

        #endregion

        public string BrowseForLocation(string path, string defaultFolder = null)
        {
            if (IsInDesignMode)
                return "";

            Logger?.Debug($"BrowseForLocation({path})");

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

            Logger?.Debug($"SelectFileFromSystem({path})");

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
