using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using MultiTemplateGenerator.UI.Helpers;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class OptionsViewModel : ObservableObject
    {
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private IBaseTheme _baseTheme;
        private bool _isDarkMode;
        private RelayCommand _closeCommand;
        private bool _autoExpand;
        private bool _copyPropertiesFromSolution;
        private string _excludedFolders;
        private readonly string defaultExcludedFolders = ".*;bin;obj;TestResults;node_modules";
        public OptionsViewModel()
        {
            IsDarkMode = false;
            _excludedFolders = defaultExcludedFolders;
        }

        public bool AutoExpand
        {
            get => _autoExpand;
            set { _autoExpand = value; RaisePropertyChanged(); }
        }

        public bool CopyPropertiesFromSolution
        {
            get => _copyPropertiesFromSolution;
            set { _copyPropertiesFromSolution = value; RaisePropertyChanged(); }
        }

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

        public string ExcludedFolders
        {
            get => _excludedFolders;
            set { _excludedFolders = value; RaisePropertyChanged(); }
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
