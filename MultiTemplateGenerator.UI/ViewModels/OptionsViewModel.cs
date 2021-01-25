using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using MultiTemplateGenerator.UI.Helpers;
using Serilog;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class OptionsViewModel : ObservableObject
    {
        public ILogger Logger { get; }
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private IBaseTheme _baseTheme;
        private bool _isDarkMode;
        private RelayCommand _closeCommand;

        public OptionsViewModel(ILogger logger)
        {
            Logger = logger;

            IsDarkMode = false;
        }

        public bool AutoExpand { get; set; } = true;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                ToggleDarkLightTheme(_isDarkMode);
                RaisePropertyChanged();
            }
        }

        public IBaseTheme BaseTheme
        {
            get => _baseTheme;
            set
            {
                _baseTheme = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand CloseCommand => _closeCommand ??= new RelayCommand(() =>
        {
            DialogHost.Close(ViewNames.DialogRoot);
        }, () => true);

        private void ToggleDarkLightTheme(bool isDark)
        {
            ITheme theme = _paletteHelper.GetTheme();
            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);

            BaseTheme = baseTheme;
        }
    }
}
