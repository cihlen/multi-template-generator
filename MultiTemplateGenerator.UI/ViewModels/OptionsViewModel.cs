﻿
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private string _excludedFiles;
        private readonly string defaultExcludedFiles = "*.user";

        public OptionsViewModel()
        {
            IsDarkMode = false;
            _excludedFolders = defaultExcludedFolders;
            _excludedFiles = defaultExcludedFiles;
        }

        public bool AutoExpand
        {
            get => _autoExpand;
            set { _autoExpand = value; OnPropertyChanged(); }
        }

        public bool CopyPropertiesFromSolution
        {
            get => _copyPropertiesFromSolution;
            set { _copyPropertiesFromSolution = value; OnPropertyChanged(); }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                ToggleDarkLightTheme(_isDarkMode);
                OnPropertyChanged();
            }
        }

        public IBaseTheme BaseTheme
        {
            get => _baseTheme;
            set
            {
                _baseTheme = value;
                OnPropertyChanged();
            }
        }

        public string ExcludedFolders
        {
            get => _excludedFolders;
            set { _excludedFolders = value; OnPropertyChanged(); }
        }

        public string ExcludedFiles
        {
            get => _excludedFiles;
            set { _excludedFiles = value; OnPropertyChanged(); }
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
