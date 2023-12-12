using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using MultiTemplateGenerator.UI.Helpers;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class AboutViewModel : CommonViewModel
    {
        private RelayCommand _closeCommand;

        public AboutViewModel(ILogger logger, bool isDarkMode) : base(logger)
        {
            IsDarkMode = isDarkMode;
            AppVersion = AppHelper.FileVersion;
        }

        public bool IsDarkMode { get; }

        public string AppVersion { get; }

        public RelayCommand CloseCommand => _closeCommand ??= new RelayCommand(() =>
        {
            DialogHost.Close(ViewNames.DialogRoot);
        }, () => true);
    }
}
