using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MultiTemplateGenerator.UI.Properties;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.UI.Helpers;

namespace MultiTemplateGenerator.UI.Models
{
    public class ProjectTemplateModel : ViewModelBase, IChecked, IProjectTemplate
    {
        private readonly Action<ProjectTemplateModel, string> _propertyChanged;
        private RelayCommand _browseIcon;
        private RelayCommand _browsePreviewImage;

        private RelayCommand _resetPlatformTagsCommand;
        private RelayCommand _resetProjectTypeTagsCommand;

        private readonly OpenFileDialog _openImageDialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = @"Image files (*.ico,*.png,*.jpg,*.jpeg,*.bmp)|*.ico;*.png;*.jpg;*.jpeg;*.bmp"
        };

        private string _templateFileName;
        private string _templateName;
        private string _description = "<No description available>";
        private string _defaultName;
        private string _languageTag;
        private string _projectSubType;
        private string _iconImagePath;
        private string _previewImagePath;
        private bool _isHidden;
        private bool _isChecked = true;

        private string _projectTypeTags;
        private ObservableCollection<CheckedItemModel> _platformTagsList;
        private string _platformTags;
        private ObservableCollection<CheckedItemModel> _projectTypeTagsList;
        public static List<string> DefaultProjectTypeTags { get; }
        public static List<string> DefaultPlatformTags { get; }
        public static List<string> DefaultLanguageTags { get; }
        public List<string> LanguageTags { get; }

        private bool _isBusy;
        private bool _createNewFolder = true;
        private bool _enableLocationBrowseButton = true;
        private bool _createInPlace = true;
        private bool _provideDefaultName = true;
        private LocationFieldType _locationField = LocationFieldType.Enabled;
        private string _maxFrameworkVersion;
        private string _requiredFrameworkVersion;
        private string _frameworkVersion;
        private int _sortOrder = 1000;
        private string _projectFileName;
        private ImageSource _iconImageSource;
        private ImageSource _previewImageSource;
        private ImageSource _itemImage;

        private static readonly BitmapSource SolutionFolderIcon;

        static ProjectTemplateModel()
        {
            DefaultPlatformTags = Settings.Default.DefaultPlatformTags.GetTags();
            DefaultProjectTypeTags = Settings.Default.DefaultProjectTypeTags.GetTags();
            DefaultLanguageTags = Settings.Default.DefaultLanguageTags.GetTags();
            
            SolutionFolderIcon = Resources.folder_icon_32.ToImageSource();
        }

        //public ProjectTemplateModel()
        //{
        //    PlatformTagsList = new ObservableCollection<CheckedItemModel>(DefaultPlatformTags.CombineCheckedItems(Settings.Default.PlatformTags, PlatformTagCheckedChanged));
        //    ProjectTypeTagsList = new ObservableCollection<CheckedItemModel>(DefaultProjectTypeTags.CombineCheckedItems(Settings.Default.ProjectTypeTags, ProjectTypeTagCheckedChanged));

        //    LanguageTags = new List<string>(DefaultLanguageTags);
        //    IsChecked = true;
        //}

        public ProjectTemplateModel(bool isProject, ProjectTemplateModel parent, Action<ProjectTemplateModel, string> propertyChanged)
        {
            _propertyChanged = propertyChanged;
            PlatformTagsList = new ObservableCollection<CheckedItemModel>(DefaultPlatformTags.CombineCheckedItems(AppSettings.SolutionTemplateSettings.PlatformTags, PlatformTagCheckedChanged));
            ProjectTypeTagsList = new ObservableCollection<CheckedItemModel>(DefaultProjectTypeTags.CombineCheckedItems(AppSettings.SolutionTemplateSettings.ProjectTypeTags, ProjectTypeTagCheckedChanged));

            LanguageTags = new List<string>(DefaultLanguageTags);
            _isChecked = true;
            IsProject = isProject;
            Parent = parent;
        }
        
        public bool IsBusy
        {
            get => _isBusy;
            internal set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                    return;

                _isChecked = value;
                RaisePropertyChanged();
                
                foreach (var child in Children.Cast<ProjectTemplateModel>())
                {
                    child.IsChecked = _isChecked;
                }

                if (_isChecked)
                {
                    Parent?.SetParentChecked(true);
                }

                _propertyChanged?.Invoke(this, nameof(IsChecked));
            }
        }

        public void SetParentChecked(bool isChecked)
        {
            _isChecked = isChecked;
            RaisePropertyChanged(nameof(IsChecked));

            Parent?.SetParentChecked(isChecked);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set { _sortOrder = value; RaisePropertyChanged(); }
        }

        public bool CreateNewFolder
        {
            get => _createNewFolder;
            set { _createNewFolder = value; RaisePropertyChanged(); }
        }

        public string FrameworkVersion
        {
            get => _frameworkVersion;
            set { _frameworkVersion = value; RaisePropertyChanged(); }
        }

        public string RequiredFrameworkVersion
        {
            get => _requiredFrameworkVersion;
            set { _requiredFrameworkVersion = value; RaisePropertyChanged(); }
        }

        public string MaxFrameworkVersion
        {
            get => _maxFrameworkVersion;
            set { _maxFrameworkVersion = value; RaisePropertyChanged(); }
        }

        public Array LocationFieldTypes => Enum.GetValues(typeof(LocationFieldType));

        public bool ProvideDefaultName
        {
            get => _provideDefaultName;
            set { _provideDefaultName = value; RaisePropertyChanged(); }
        }

        public bool CreateInPlace
        {
            get => _createInPlace;
            set { _createInPlace = value; RaisePropertyChanged(); }
        }

        public bool EnableLocationBrowseButton
        {
            get => _enableLocationBrowseButton;
            set { _enableLocationBrowseButton = value; RaisePropertyChanged(); }
        }

        public LocationFieldType LocationField
        {
            get => _locationField;
            set { _locationField = value; RaisePropertyChanged(); }
        }

        public string ProjectFileName
        {
            get => _projectFileName;
            set
            {
                _projectFileName = value;
                if (!string.IsNullOrWhiteSpace(_projectFileName) && string.IsNullOrEmpty(LanguageTag))
                    LanguageTag = _projectFileName.GetLanguageTagFromExtension();

                RaisePropertyChanged(nameof(ProjectType));
            }
        }

        public ImageSource ItemImage
        {
            get => _itemImage;
            private set { _itemImage = value; RaisePropertyChanged(); }
        }

        public string TemplateName
        {
            get => _templateName;
            set
            {
                if (_templateName == value)
                    return;

                _templateName = value;
                RaisePropertyChanged();

                TemplateFileName = TemplateName.GetSafeFileName() + ".vstemplate";
                RaisePropertyChanged(nameof(TemplateFileName));

                _propertyChanged?.Invoke(this, nameof(TemplateName));
            }
        }

        public string TemplateFileName
        {
            get => _templateFileName;
            set { _templateFileName = value; RaisePropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                if (string.IsNullOrWhiteSpace(_description))
                {
                    _description = "<No description available>";
                }
                RaisePropertyChanged();
            }
        }

        public string DefaultName
        {
            get => _defaultName;
            set { _defaultName = value; RaisePropertyChanged(); }
        }

        public string ProjectType => ProjectFileName.GetProjectTypeFromExtension() ?? LanguageTag.GetProjectTypeFromExtension();

        public string LanguageTag
        {
            get => _languageTag;
            set
            {
                if (_languageTag == value)
                    return;

                _languageTag = value;
                RaisePropertyChanged();

                RaisePropertyChanged(nameof(ProjectType));
                _propertyChanged?.Invoke(this, nameof(LanguageTag));
            }
        }

        public ObservableCollection<CheckedItemModel> PlatformTagsList
        {
            get => _platformTagsList;
            set { _platformTagsList = value; RaisePropertyChanged(); }
        }

        public string PlatformTags
        {
            get => _platformTags;
            set
            {
                if (_platformTags == value)
                    return;

                _platformTags = value;
                RaisePropertyChanged();

                PlatformTagsList.GetNewCheckedItems(_platformTags, PlatformTagCheckedChanged);
                RaisePropertyChanged(nameof(PlatformTagsList));
            }
        }

        private void PlatformTagCheckedChanged(CheckedItemModel item)
        {
            PlatformTags = PlatformTagsList.GetCheckedItemsString();
        }

        public ObservableCollection<CheckedItemModel> ProjectTypeTagsList
        {
            get => _projectTypeTagsList;
            set { _projectTypeTagsList = value; RaisePropertyChanged(); }
        }

        public string ProjectTypeTags
        {
            get => _projectTypeTags;
            set
            {
                if (_projectTypeTags == value)
                    return;

                _projectTypeTags = value;
                RaisePropertyChanged();

                ProjectTypeTagsList.GetNewCheckedItems(_projectTypeTags, ProjectTypeTagCheckedChanged);
                RaisePropertyChanged(nameof(ProjectTypeTagsList));
            }
        }

        private void ProjectTypeTagCheckedChanged(CheckedItemModel item)
        {
            ProjectTypeTags = ProjectTypeTagsList.GetCheckedItemsString();
        }

        public string ProjectSubType
        {
            get => _projectSubType;
            set { _projectSubType = value; RaisePropertyChanged(); }
        }

        public ImageSource IconImageSource
        {
            get => _iconImageSource;
            private set
            {
                _iconImageSource = value;
                RaisePropertyChanged();
                SetItemImage();
            }
        }

        public string IconImagePath
        {
            get => _iconImagePath;
            set
            {
                _iconImagePath = value;
                RaisePropertyChanged();

                InvokeHelper.Invoke(() =>
                {
                    IconImageSource = _iconImagePath.FileExists() ? _iconImagePath.ToImageSource() : null;
                });
            }
        }

        public ImageSource PreviewImageSource
        {
            get => _previewImageSource;
            private set { _previewImageSource = value; RaisePropertyChanged(); }
        }

        public string PreviewImagePath
        {
            get => _previewImagePath;
            set
            {
                _previewImagePath = value;
                RaisePropertyChanged();

                InvokeHelper.Invoke(() =>
                {
                    PreviewImageSource = _previewImagePath.FileExists() ? _previewImagePath.ToImageSource() : null;
                });
            }
        }

        public bool IsHidden
        {
            get => _isHidden;
            set { _isHidden = value; RaisePropertyChanged(); }
        }

        public bool IsProject { get; }
        public ProjectTemplateModel Parent { get; }

        public bool IsSolutionFolder => !IsProject;

        public List<IProjectTemplate> Children { get; internal set; } = new List<IProjectTemplate>();

        public RelayCommand BrowseIcon => _browseIcon ??= new RelayCommand(() =>
        {
            if (IsInDesignMode)
                return;

            if (_openImageDialog.ShowDialogPath(IconImagePath) != DialogResult.OK)
                return;

            IconImagePath = _openImageDialog.FileName;
        }, () => !IsBusy);

        public RelayCommand BrowsePreviewImage => _browsePreviewImage ??= new RelayCommand(() =>
        {
            if (IsInDesignMode)
                return;

            if (_openImageDialog.ShowDialogPath(PreviewImagePath) != DialogResult.OK)
                return;

            PreviewImagePath = _openImageDialog.FileName;
        }, () => !IsBusy);

        public RelayCommand ResetPlatformTagsCommand => _resetPlatformTagsCommand ??= new RelayCommand(ResetPlatformTags, () => !IsBusy);

        private void ResetPlatformTags()
        {
            if (!$"Are you sure you want to reset {PlatformTagsList.Count} Platform tags?".ShowQuestion("Reset tags"))
                return;

            PlatformTagsList = new ObservableCollection<CheckedItemModel>(DefaultPlatformTags.CombineCheckedItems(string.Empty, PlatformTagCheckedChanged));
            PlatformTags = string.Empty;
        }

        public RelayCommand ResetProjectTypeTagsCommand => _resetProjectTypeTagsCommand ??= new RelayCommand(ResetProjectTypeTags, () => !IsBusy);

        private void ResetProjectTypeTags()
        {
            if (!$"Are you sure you want to reset {ProjectTypeTagsList.Count} Project Type tags?".ShowQuestion("Reset tags"))
                return;

            ProjectTypeTagsList = new ObservableCollection<CheckedItemModel>(DefaultProjectTypeTags.CombineCheckedItems(string.Empty, ProjectTypeTagCheckedChanged));
            ProjectTypeTags = string.Empty;
        }

        public void SetItemImage()
        {
            ItemImage = IsProject
                ? IconImageSource
                : SolutionFolderIcon;
        }

        public override string ToString()
        {
            return "Name=" + TemplateName;
        }
    }
}
