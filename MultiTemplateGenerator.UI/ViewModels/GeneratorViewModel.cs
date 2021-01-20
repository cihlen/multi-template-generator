using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using MultiTemplateGenerator.UI.Models;
using MultiTemplateGenerator.UI.Properties;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.Lib.Generator;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.UI.Helpers;
using Serilog;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class GeneratorViewModel : CommonViewModel
    {
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private bool _isDarkMode = true;
        private IBaseTheme _baseTheme;

        private readonly ITemplateGeneratorService _generatorService;

        private ProjectTemplateModel _solutionTemplate;
        private bool _isBusy;
        private bool _autoImportToVS;
        private string _destinationFolder;
        private bool _useSolution;
        private bool _autoOpenTemplate;
        private bool _isWindowOpen;
        private string _solutionFile;
        private string _currentVsTemplateFolder;

        protected CancellationTokenSource CancelSource;

        private bool _isAppMenuOpen;
        private RelayCommand _cancelCommand;
        private RelayCommand<ProjectTemplateModel> _openProjectDetailsCommand;
        private RelayCommand _browseDestinationFolder;
        private RelayCommand _browseSolution;
        private RelayCommand _generateTemplatesCommand;
        private RelayCommand<object> _closeWindowCommand;
        private RelayCommand _refreshViewCommand;
        private RelayCommand<string> _openDialogCommand;

        public GeneratorViewModel(ITemplateGeneratorService generatorService, ILogger logger)
            : base(logger)
        {
            try
            {
                _generatorService = generatorService;

                GetSettings();

                ProjectItems.CollectionChanged += NotifyCollectionChangedEventHandler;

                logger.Information("{method} initialized", nameof(GeneratorViewModel));
            }
            catch (Exception e)
            {
                logger?.Error(e, "Error initializing {name}", nameof(GeneratorViewModel));
                throw;
            }
        }

        private void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetProjectCounts();
        }

        private void GetSettings()
        {
            VSTemplateFolder = FileExtensions.FindVSTemplateFolder();

            IsDarkMode = Settings.Default.IsDarkMode;

            _destinationFolder = Settings.Default.OutputPath;

            if (string.IsNullOrWhiteSpace(_destinationFolder))
            {
                _destinationFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                     @"\Multi-Template Generator\Output";
            }

            SolutionTemplate = new ProjectTemplateModel(false, null, SolutionPropertyChanged)
            {
                CreateNewFolder = true
            };

            AppSettings.SolutionTemplateSettings.CopyTemplateProperties(SolutionTemplate);

            var platformTags = SolutionTemplate.PlatformTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            if (platformTags.Count == 0)
            {
                platformTags = new List<string> { ProjectTemplateModel.DefaultPlatformTags[6] };
            }
            foreach (var platformTag in SolutionTemplate.PlatformTagsList)
            {
                platformTag.IsChecked = platformTags.Contains(platformTag.Text);
            }

            var projectTypeTags = SolutionTemplate.ProjectTypeTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            if (projectTypeTags.Count == 0)
            {
                projectTypeTags = new List<string> { ProjectTemplateModel.DefaultProjectTypeTags[2] };
            }
            foreach (var projectTypeTag in SolutionTemplate.ProjectTypeTagsList)
            {
                projectTypeTag.IsChecked = projectTypeTags.Contains(projectTypeTag.Text);
            }

            AutoOpenTemplate = Settings.Default.OpenGeneratedTemplate;
            AutoImportToVS = Settings.Default.AutoImportToVS;
        }

        public string VSTemplateFolder { get; private set; }

        public void SolutionPropertyChanged(ProjectTemplateModel template, string propertyName)
        {
            try
            {
                if (propertyName.Equals(nameof(template.TemplateName)))
                {
                    RaisePropertyChanged(nameof(SolutionTemplateFullPath));
                }
                else if (propertyName.Equals(nameof(template.LanguageTag)))
                {
                    CurrentVSTemplateFolder =
                        Path.Combine(VSTemplateFolder, template.LanguageTag.GetTemplateFolderNameByLanguage());
                }
            }
            catch (Exception e)
            {
                SetError(e);
            }
        }

        public void SaveSettings()
        {
            Settings.Default.OpenGeneratedTemplate = AutoOpenTemplate;
            Settings.Default.AutoImportToVS = AutoImportToVS;
            Settings.Default.OutputPath = DestinationFolder;
            Settings.Default.IsDarkMode = IsDarkMode;

            SolutionTemplate.TrimProperties();
            SolutionTemplate.CopyTemplateProperties(AppSettings.SolutionTemplateSettings);
            AppSettings.SaveSettings();

            Settings.Default.Save();
        }

        public int GeneratedCount { get; private set; }

        public RelayCommand BrowseSolution => _browseSolution ??= new RelayCommand(() =>
        {
            if (IsInDesignMode)
                return;

            var filePath = SelectFileFromSystem(SolutionFile,
                string.IsNullOrWhiteSpace(SolutionFile) ? string.Empty : Path.GetDirectoryName(SolutionFile),
                "Select Solution file", ".sln", "Solution file (*.sln, *.vstemplate)|*.sln;*.vstemplate");
            if (filePath != null)
            {
                SetSolutionFile(filePath);
            }
        }, () => !IsBusy);

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
            set { _baseTheme = value; RaisePropertyChanged(); }
        }

        private void ToggleDarkLightTheme(bool isDark)
        {
            ITheme theme = _paletteHelper.GetTheme();
            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);

            BaseTheme = baseTheme;
        }

        public string SolutionFile
        {
            get => _solutionFile;
            set { _solutionFile = value; RaisePropertyChanged(); }
        }

        public string SolutionTemplateFullPath =>
            DestinationFolder.GetTargetTemplatePath(SolutionTemplate?.TemplateName);

        private bool IsFileSolution => !string.IsNullOrWhiteSpace(SolutionFile) && Path.GetExtension(SolutionFile).Equals(".sln", StringComparison.InvariantCultureIgnoreCase);

        public ProjectTemplateModel SolutionTemplate
        {
            get => _solutionTemplate;
            private set
            {
                _solutionTemplate = value;
                RaisePropertyChanged();

                RaisePropertyChanged(nameof(SolutionTemplateFullPath));
            }
        }

        public ObservableCollection<ProjectTemplateModel> ProjectItems { get; } = new ObservableCollection<ProjectTemplateModel>();

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnUpdateButtonsCanExecute();
            }
        }

        public bool IsWindowOpen
        {
            get => _isWindowOpen;
            set => Set(ref _isWindowOpen, value);
        }

        public string CurrentVSTemplateFolder
        {
            get => _currentVsTemplateFolder;
            private set { _currentVsTemplateFolder = value; RaisePropertyChanged(); }
        }



        public bool AutoImportToVS
        {
            get => _autoImportToVS;
            set { _autoImportToVS = value; RaisePropertyChanged(); }
        }

        public bool AutoOpenTemplate
        {
            get => _autoOpenTemplate;
            set { _autoOpenTemplate = value; RaisePropertyChanged(); }
        }

        public string DestinationFolder
        {
            get => _destinationFolder;
            set
            {
                if (_destinationFolder == value)
                    return;

                _destinationFolder = value;
                RaisePropertyChanged();

                try
                {
                    if (!UseSolution)
                        SetSolutionItems();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }
        }

        public bool UseSolution
        {
            get => _useSolution;
            set
            {
                _useSolution = value;
                RaisePropertyChanged();

                if (IsInDesignMode)
                    return;

                SetSolutionItems();
            }
        }

        public RelayCommand BrowseDestinationFolder => _browseDestinationFolder ??= new RelayCommand(() =>
        {
            if (IsInDesignMode)
                return;

            var folder = BrowseForLocation(DestinationFolder);
            if (folder != null)
                DestinationFolder = folder;
        }, () => !IsBusy);

        public RelayCommand GenerateTemplatesCommand => _generateTemplatesCommand ??= new RelayCommand(async () => await GenerateTemplateAsync(), () => !IsBusy);

        public RelayCommand<object> CloseWindowCommand => _closeWindowCommand ??= new RelayCommand<object>(window =>
        {
            if (IsBusy)
            {
                //Cancel
                CancelOperation();
            }

            SaveSettings();

            ((Window)window).Close();
        }, window => window is Window && !IsBusy || CanCancelOperation());

        public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(() =>
        {
            CancelOperation();
            DialogHost.Close(ViewNames.DialogRoot);
            IsWindowOpen = false;

        }, () => IsBusy);

        public RelayCommand<ProjectTemplateModel> OpenProjectDetailsCommand => _openProjectDetailsCommand
            ??= new RelayCommand<ProjectTemplateModel>(async (model) => await OpenProjectDetails(model), (model) => !IsBusy && !IsWindowOpen && model?.IsProject == true);

        public RelayCommand RefreshViewCommand => _refreshViewCommand ??= new RelayCommand(SetSolutionItems, () => !IsBusy && !IsWindowOpen);

        private RelayCommand<string> _deleteFolderContentCommand;
        private int _projectTemplateCount;
        private int _solutionFolderCount;

        public RelayCommand<string> DeleteFolderContentCommand => _deleteFolderContentCommand ??= new RelayCommand<string>(DeleteFolderContent,
            (location) => !IsInDesignMode && !IsBusy && location.DirectoryExists());

        protected void DeleteFolderContent(string location)
        {
            try
            {
                if (!location.DirectoryOrFileExists())
                    return;

                var di = new DirectoryInfo(location);
                var totalDirs = di.GetDirectories("*.*", SearchOption.AllDirectories);
                var totalFiles = di.GetFiles("*.*", SearchOption.AllDirectories);
                if (totalDirs.Length == 0 && totalFiles.Length == 0)
                    return;
                var totalSize = totalFiles.Sum(x => x.Length);

                if (!$"Are you sure you want to delete the entire folder contents?\n\n{location}:\nDirectories: {totalDirs.Length}\nFiles: {totalFiles.Length}\nSize: {totalSize.ToFileSize()}"
                    .ShowQuestion("Delete folder content"))
                    return;

                location.DeleteDirectoryContents();

                Logger.Information("Folder content deleted: {folder}", location);

                if (!UseSolution)
                {
                    SetSolutionItems();
                }

                "Folder content deleted.".ShowWarning("Delete folder content");
            }
            catch (Exception e)
            {
                SetError(e, "Error in DeleteFolderContent");
            }
        }

        public bool IsAppMenuOpen
        {
            get => _isAppMenuOpen;
            set { _isAppMenuOpen = value; RaisePropertyChanged(); }
        }

        public RelayCommand<string> OpenDialogCommand => _openDialogCommand ??= new RelayCommand<string>(async (dialogName) => await OpenAppDialog(dialogName),
            (dialogName) => !IsBusy && !IsWindowOpen && !string.IsNullOrEmpty(dialogName));

        public int ProjectTemplateCount
        {
            get => _projectTemplateCount;
            private set { _projectTemplateCount = value; RaisePropertyChanged(); }
        }

        public int SolutionFolderCount
        {
            get => _solutionFolderCount;
            private set { _solutionFolderCount = value; RaisePropertyChanged(); }
        }

        private async Task OpenAppDialog(string dialogName)
        {
            IsWindowOpen = true;
            try
            {
                switch (dialogName)
                {
                    case ViewNames.About:
                        {
                            await DialogHost.Show(new AboutViewModel(Logger, IsDarkMode), ViewNames.DialogRoot);
                        }
                        break;
                    case ViewNames.Busy:
                        {
                            await DialogHost.Show(new BusyViewModel(this), ViewNames.DialogRoot);
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                SetError(exception, $"Error in {nameof(OpenAppDialog)}");
            }
            finally
            {
                IsWindowOpen = false;
            }
        }

        private async Task OpenProjectDetails(ProjectTemplateModel model)
        {
            IsWindowOpen = true;
            try
            {
                var vm = new ProjectDetailsViewModel(model, Logger);
                await DialogHost.Show(vm, "DialogRoot");
            }
            catch (Exception exception)
            {
                SetError(exception, $"Error in {nameof(OpenProjectDetails)}");
            }
            finally
            {
                IsWindowOpen = false;
            }
        }

        public void SetSolutionFile(string solutionFile)
        {
            SolutionFile = solutionFile;
            UseSolution = SolutionFile.FileExists();
        }

        internal TemplateOptions GetTemplateOptions()
        {
            return new TemplateOptions
            {
                SolutionFolder = Path.GetDirectoryName(SolutionFile),
                SolutionTemplate = SolutionTemplate,
                ProjectTemplates = ProjectItems.ConvertCheckedProjectTemplates(),
                TargetFolder = DestinationFolder,
                UseSolution = UseSolution,
                AutoImportToVS = AutoImportToVS
            };
        }

        private void SetSolutionItems()
        {
            try
            {
                ProjectItems.Clear();
                List<IProjectTemplate> projectItems = new List<IProjectTemplate>(0);

                string defaultTemplateName = string.IsNullOrWhiteSpace(SolutionFile)
                    ? null
                    : Path.GetFileNameWithoutExtension(SolutionFile)
                        .Replace('.', ' ');

                if (!string.IsNullOrEmpty(defaultTemplateName))
                    SolutionTemplate.TemplateName = defaultTemplateName;
                SolutionTemplate.DefaultName = defaultTemplateName ?? SolutionTemplate.TemplateName;

                if (UseSolution)
                {
                    if (!SolutionFile.FileExists())
                    {
                        return;
                    }

                    if (IsFileSolution)
                    {
                        projectItems = _generatorService.GetProjectTemplates(SolutionFile, SolutionTemplate);
                    }
                    else
                    {
                        var solutionTemplate = _generatorService.ReadSolutionTemplate(SolutionFile);
                        projectItems.AddRange(solutionTemplate.Children);
                        solutionTemplate.Children.Clear();
                        SolutionTemplate = solutionTemplate.ToModel(null);
                    }
                }
                else
                {
                    if (DestinationFolder.DirectoryExists())
                    {
                        Settings.Default.OutputPath = DestinationFolder;
                        projectItems = _generatorService.GetProjectTemplatesFromFolder(DestinationFolder).ToList();
                    }
                }

                projectItems = projectItems.OrderByDescending(x => x.IsProject).ToList();

                foreach (var projectTemplate in projectItems.ToModels())
                {
                    ProjectItems.Add(projectTemplate);
                }
            }
            catch (Exception e)
            {
                SetError(e);
            }
            finally
            {
                RaisePropertyChanged(nameof(ProjectItems));
                SetProjectCounts();
            }
        }

        private void SetProjectCounts()
        {
            var flattened = ProjectItems.GetTemplatesFlattened();
            ProjectTemplateCount = flattened.Count(x => x.IsProject);
            SolutionFolderCount = flattened.Count(x => !x.IsProject);
        }

        #region Cancellation

        protected bool CanCancelOperation()
        {
            return CancelSource?.IsCancellationRequested == false;
        }

        protected void CancelOperation()
        {
            InvokeHelper.Invoke(() =>
            {
                if (CancelSource == null)
                {
                    SetError("Nothing to cancel!");
                    return;
                }

                if (CancelSource.IsCancellationRequested)
                {
                    //SetError(null, null, "Cancellation already in progress!");
                    return;
                }

                try
                {
                    //SetWarn(null, "Cancelling...");
                    //OnCancelling();
                    CancelSource.Cancel();
                }
                catch (Exception e)
                {
                    SetError($"Error cancelling: {e.Message}");
                }

                CloseWindowCommand.RaiseCanExecuteChanged();
            });
        }

        protected void DisposeCancelSource()
        {
            InvokeHelper.Invoke(() =>
            {
                if (CancelSource != null)
                {
                    CancelSource?.Dispose();
                    CancelSource = null;
                    OnUpdateButtonsCanExecute();
                }
            });
        }

        private void OnUpdateButtonsCanExecute()
        {
            InvokeHelper.Invoke(() =>
            {
                RaisePropertyChanged(nameof(IsBusy));
                RaisePropertyChanged(nameof(IsWindowOpen));

                OpenProjectDetailsCommand.RaiseCanExecuteChanged();
                CloseWindowCommand.RaiseCanExecuteChanged();
                GenerateTemplatesCommand.RaiseCanExecuteChanged();
                SolutionTemplate.BrowseIcon.RaiseCanExecuteChanged();
                SolutionTemplate.BrowsePreviewImage.RaiseCanExecuteChanged();
                BrowseDestinationFolder.RaiseCanExecuteChanged();
            });
        }

        protected void NewCancelSource()
        {
            InvokeHelper.Invoke(() =>
            {
                if (CancelSource != null)
                    throw new ArgumentException("Cancellation Token is already begin used!");

                CancelSource = new CancellationTokenSource();
                OnUpdateButtonsCanExecute();
            });
        }

        #endregion

        private async Task GenerateTemplateAsync()
        {
            try
            {
                IsBusy = true;
                NewCancelSource();

                GeneratedCount = 0;

                var errors = ValidateInputs();

                if (errors.Count != 0)
                {
                    var errorMessage = string.Join(Environment.NewLine, errors);
                    errorMessage.ShowErrorMessageBox();
                    return;
                }

                string existingContentMsg = string.Empty;
                if (UseSolution)
                {
                    var dirCount = Directory.EnumerateDirectories(DestinationFolder).Count();
                    CancelSource.Token.ThrowIfCancellationRequested();
                    var fileCount = Directory.EnumerateFiles(DestinationFolder).Count();
                    CancelSource.Token.ThrowIfCancellationRequested();
                    if (dirCount != 0 || fileCount != 0)
                    {
                        existingContentMsg = "WARNING: Output folder already contains ";
                        if (dirCount != 0)
                        {
                            existingContentMsg += $"{dirCount} directories";
                            if (fileCount != 0)
                                existingContentMsg += " and ";
                        }

                        if (fileCount != 0)
                        {
                            existingContentMsg += $"{fileCount} files";
                        }

                        existingContentMsg += $".{Environment.NewLine}Existing files will be overwritten!{Environment.NewLine}{Environment.NewLine}";
                    }
                }

                var options = GetTemplateOptions();
                var flattenedProjectTemplates = options.ProjectTemplates.GetTemplatesFlattened();
                var solutionFolderCount = flattenedProjectTemplates.Count(x => !x.IsProject);
                var projectCount = flattenedProjectTemplates.Count(x => x.IsProject);

                var confirmContentMsg = $"{existingContentMsg}Are you sure you want to generate {projectCount} project templates";
                if (solutionFolderCount != 0)
                    confirmContentMsg += $" and {solutionFolderCount} solution folders";
                confirmContentMsg += "?";

                if (!confirmContentMsg.ShowQuestion("Generate Templates"))
                {
                    return;
                }

                SaveSettings();

                var multiTemplateFile = new FileInfo(SolutionTemplateFullPath);
                if (multiTemplateFile.Exists && string.IsNullOrEmpty(existingContentMsg))
                {
                    if (!$"Are you sure you want to overwrite {multiTemplateFile.FullName}?".ShowQuestion(@"Overwrite Template"))
                    {
                        return;
                    }

                    multiTemplateFile.Delete();
                }

                ShowBusyWindow();

                await Task.Run(() =>
                {
                    _generatorService.GenerateTemplate(options, CancelSource.Token);
                    GeneratedCount = flattenedProjectTemplates.Count;
                }, CancelSource.Token);

                DisposeCancelSource();

                CloseBusyWindow();

                if (AutoOpenTemplate)
                {
                    try
                    {
                        Process.Start(multiTemplateFile.FullName);
                    }
                    catch (Exception e)
                    {
                        SetError(e, $"Error opening template: {e.Message}");
                    }
                }

                Logger.Information($"Generated {projectCount} projects and {solutionFolderCount} successfully", projectCount, solutionFolderCount);
                $"Generated {projectCount} projects successfully".ShowInfo(AppHelper.ProductName);
            }
            catch (OperationCanceledException)
            {
                CloseBusyWindow();
                Logger.Information("Generating templates canceled!");

                "Generating templates canceled!".ShowWarning(AppHelper.ProductName);
            }
            catch (Exception ex)
            {
                CloseBusyWindow();
                SetError(ex, $"Error generating templates: {ex.Message}");
            }
            finally
            {
                DisposeCancelSource();
                IsBusy = false;
                CloseBusyWindow();
                RaisePropertyChanged(nameof(SolutionTemplateFullPath));
                SetProjectCounts();
            }
        }

        private void ShowBusyWindow()
        {
            OpenAppDialog(ViewNames.Busy).Wait(1);
        }

        private void CloseBusyWindow()
        {
            if (!IsWindowOpen)
                return;

            IsWindowOpen = false;
            try
            {
                DialogHost.Close("DialogRoot");
            }
            catch (Exception e)
            {
                SetError(e, $"Error in {nameof(CloseBusyWindow)}");
            }
        }

        private List<string> ValidateInputs()
        {
            var errors = SolutionTemplate.ValidateInputs();

            DestinationFolder = DestinationFolder.Trim();
            if (string.IsNullOrWhiteSpace(DestinationFolder))
            {
                errors.Add(@"Output folder is missing.");
            }
            else if (!DestinationFolder.DirectoryExists())
            {
                DestinationFolder.CreateDirectory();
            }

            if (errors.Any())
            {
                errors.Insert(0, "Solution Template: " + SolutionTemplate.TemplateName);
            }

            var flattenedProjectTemplates = ProjectItems.ConvertCheckedProjectTemplates().GetTemplatesFlattened();
            var projectCount = flattenedProjectTemplates.Count(x => x.IsProject);
            foreach (var projectTemplate in flattenedProjectTemplates)
            {
                var projectErrors = projectTemplate.ValidateInputs();
                if (projectErrors.Any())
                {
                    projectErrors.Insert(0, "Template: " + projectTemplate.TemplateName);
                    errors.AddRange(projectErrors);
                }
            }

            if (projectCount == 0)
            {
                errors.Add(@"You have to select at least one project.");
            }

            return errors;
        }


    }
}
