using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using MultiTemplateGenerator.UI.Models;
using MultiTemplateGenerator.UI.Properties;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.Lib.Generator;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.UI.Helpers;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class GeneratorViewModel : CommonViewModel
    {
        private readonly ITemplateGeneratorService _generatorService;

        private ProjectTemplateModel _solutionTemplate;
        private bool _isBusy;
        private bool _autoImportToVS;
        private string _outputPath;
        private bool _autoOpenTemplate;
        private bool _isWindowOpen;
        private string _solutionFile;
        private string _currentVsTemplateFolder;
        private int _projectTemplateCount;
        private int _solutionFolderCount;

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
        private RelayCommand<string> _deleteFolderContentCommand;

        public GeneratorViewModel(ITemplateGeneratorService generatorService, ILogger<GeneratorViewModel> logger)
            : base(logger)
        {
            try
            {
                _generatorService = generatorService;

                OptionsVM = new OptionsViewModel();

                GetSettings();

                ProjectItems.CollectionChanged += NotifyCollectionChangedEventHandler;

                logger.LogInformation("{method} initialized.", nameof(GeneratorViewModel));
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Error initializing {name}.", nameof(GeneratorViewModel));
                throw;
            }
        }

        public OptionsViewModel OptionsVM { get; set; }

        #region Properties

        public bool IsAppMenuOpen
        {
            get => _isAppMenuOpen;
            set
            {
                _isAppMenuOpen = value;
                RaisePropertyChanged();
            }
        }

        public string VSTemplateFolder { get; private set; }

        public bool IsTagsSupported => _generatorService.IsTagsSupported;

        public int ProjectTemplateCount
        {
            get => _projectTemplateCount;
            private set
            {
                _projectTemplateCount = value;
                RaisePropertyChanged();
            }
        }

        public int SolutionFolderCount
        {
            get => _solutionFolderCount;
            private set
            {
                _solutionFolderCount = value;
                RaisePropertyChanged();
            }
        }

        public string TemplateTargetFolder
        {
            get
            {
                try
                {
                    return string.IsNullOrWhiteSpace(OutputPath) ||
                           string.IsNullOrWhiteSpace(SolutionTemplate?.TemplateName)
                        ? string.Empty
                        : Path.Combine(OutputPath, SolutionTemplate.TemplateName.GetSafePathName());
                }
                catch (Exception e)
                {
                    SetError(e, $"Error getting {nameof(TemplateTargetFolder)}.");
                    return null;
                }
            }
        }

        public string SolutionTemplateFullPath
        {
            get
            {
                try
                {
                    return TemplateTargetFolder.GetTargetTemplatePath(SolutionTemplate?.TemplateName);
                }
                catch (Exception e)
                {
                    SetError(e, $"Error getting {nameof(SolutionTemplateFullPath)}.");
                    return null;
                }
            }
        }

        public string OutputPath
        {
            get => _outputPath;
            set
            {
                if (_outputPath == value)
                    return;

                _outputPath = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TemplateTargetFolder));
                RaisePropertyChanged(nameof(SolutionTemplateFullPath));
            }
        }

        public string SolutionFile
        {
            get => _solutionFile;
            set
            {
                _solutionFile = value;
                RaisePropertyChanged();

                SetSolutionItems();
            }
        }

        private bool IsFileSolution => !string.IsNullOrWhiteSpace(SolutionFile)
                                       && Path.GetExtension(SolutionFile).Equals(".sln",
                                           StringComparison.InvariantCultureIgnoreCase);

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

        public ObservableCollection<ProjectTemplateModel> ProjectItems { get; } =
            new ObservableCollection<ProjectTemplateModel>();

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
            private set
            {
                _currentVsTemplateFolder = value;
                RaisePropertyChanged();
            }
        }

        public bool AutoImportToVS
        {
            get => _autoImportToVS;
            set
            {
                _autoImportToVS = value;
                RaisePropertyChanged();
            }
        }

        public bool AutoOpenTemplate
        {
            get => _autoOpenTemplate;
            set
            {
                _autoOpenTemplate = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        private void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetProjectCounts();
        }

        private void GetSettings()
        {
            VSTemplateFolder = FileExtensions.FindVSTemplateFolder();

            OptionsVM.CopyPropertiesFromSolution = Settings.Default.CopyPropertiesFromSolution;

            _outputPath = Settings.Default.OutputPath;

            if (string.IsNullOrWhiteSpace(_outputPath))
            {
                _outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                              @"\Multi-Template Generator\Output";
            }

            SolutionTemplate = new ProjectTemplateModel(false, null, SolutionPropertyChanged)
            {
                CreateNewFolder = true
            };

            AppSettings.SolutionTemplateSettings.CopyTemplateProperties(SolutionTemplate);

            var platformTags = SolutionTemplate.PlatformTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToList();
            if (platformTags.Count == 0)
            {
                platformTags = new List<string> { ProjectTemplateModel.DefaultPlatformTags[6] };
            }

            foreach (var platformTag in SolutionTemplate.PlatformTagsList)
            {
                platformTag.IsChecked = platformTags.Contains(platformTag.Text);
            }

            var projectTypeTags = SolutionTemplate.ProjectTypeTags
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
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

            OptionsVM.IsDarkMode = Settings.Default.IsDarkMode;
            OptionsVM.AutoExpand = Settings.Default.AutoExpand;

            if (!string.IsNullOrEmpty(Settings.Default.ExcludedFolders))
            {
                OptionsVM.ExcludedFolders = Settings.Default.ExcludedFolders;
            }

            if (!string.IsNullOrEmpty(Settings.Default.ExcludedFiles))
            {
                OptionsVM.ExcludedFiles = Settings.Default.ExcludedFiles;
            }
        }

        public void SaveSettings()
        {
            Settings.Default.OpenGeneratedTemplate = AutoOpenTemplate;
            Settings.Default.AutoImportToVS = AutoImportToVS;
            Settings.Default.OutputPath = OutputPath;
            Settings.Default.IsDarkMode = OptionsVM.IsDarkMode;
            Settings.Default.AutoExpand = OptionsVM.AutoExpand;
            Settings.Default.ExcludedFolders = OptionsVM.ExcludedFolders;
            Settings.Default.ExcludedFiles = OptionsVM.ExcludedFiles;
            Settings.Default.CopyPropertiesFromSolution = OptionsVM.CopyPropertiesFromSolution;

            SolutionTemplate.TrimProperties();
            SolutionTemplate.CopyTemplateProperties(AppSettings.SolutionTemplateSettings);
            AppSettings.SaveSettings();

            Settings.Default.Save();
        }

        public void SolutionPropertyChanged(ProjectTemplateModel template, string propertyName)
        {
            try
            {
                if (propertyName.Equals(nameof(template.TemplateName)))
                {
                    RaisePropertyChanged(nameof(TemplateTargetFolder));
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

        public RelayCommand BrowseDestinationFolder => _browseDestinationFolder ??= new RelayCommand(() =>
        {
            if (IsInDesignMode)
                return;

            var folder = BrowseForLocation(OutputPath);
            if (folder != null)
                OutputPath = folder;
        }, () => !IsBusy);

        public RelayCommand GenerateTemplatesCommand => _generateTemplatesCommand ??=
            new RelayCommand(async () => await GenerateTemplateAsync(), () => !IsBusy);

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
            ??= new RelayCommand<ProjectTemplateModel>(async (model) => await OpenProjectDetails(model),
                (model) => !IsBusy && !IsWindowOpen && model?.IsProject == true);

        public RelayCommand RefreshViewCommand =>
            _refreshViewCommand ??= new RelayCommand(SetSolutionItems, () => !IsBusy && !IsWindowOpen);

        public RelayCommand<string> DeleteFolderContentCommand => _deleteFolderContentCommand ??=
            new RelayCommand<string>(async (location) => await DeleteFolderContent(location),
                (location) => !IsInDesignMode && !IsBusy && location.DirectoryExists());

        public RelayCommand<string> OpenDialogCommand => _openDialogCommand ??= new RelayCommand<string>(
            async (dialogName) => await OpenAppDialog(dialogName),
            (dialogName) => !IsBusy && !IsWindowOpen && !string.IsNullOrEmpty(dialogName));

        private async Task OpenAppDialog(string dialogName)
        {
            IsWindowOpen = true;
            try
            {
                switch (dialogName)
                {
                    case ViewNames.About:
                        {
                            await DialogHost.Show(new AboutViewModel(Logger, OptionsVM.IsDarkMode), ViewNames.DialogRoot);
                        }
                        break;
                    case ViewNames.Busy:
                        {
                            await DialogHost.Show(new BusyViewModel(this), ViewNames.DialogRoot);
                        }
                        break;
                    case ViewNames.Options:
                        {
                            await DialogHost.Show(OptionsVM, ViewNames.DialogRoot);
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                await SetErrorAsync(exception, $"Error in {nameof(OpenAppDialog)}");
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
                var vm = new ProjectDetailsViewModel(model, SolutionTemplate, Logger);
                await DialogHost.Show(vm, ViewNames.DialogRoot, (sender, args) =>
                {
                    var okClicked = args.Parameter is bool parameter && parameter;
                    if (!okClicked)
                        return;

                    vm.Model.CopyTemplateProperties(model);
                });
            }
            catch (Exception exception)
            {
                await SetErrorAsync(exception, $"Error in {nameof(OpenProjectDetails)}");
            }
            finally
            {
                IsWindowOpen = false;
            }
        }

        private async Task DeleteFolderContent(string location)
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
                var dialogMessage =
                    $"Are you sure you want to delete the entire folder contents?\n\n{location}:\nDirectories: {totalDirs.Length}\nFiles: {totalFiles.Length}\nSize: {totalSize.ToFileSize()}";
                var msgBoxResult = await ShowMessageBoxAsync(dialogMessage, "Delete folder content", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);

                if (msgBoxResult != MessageBoxResult.Yes)
                    //if (!dialogMessage.ShowQuestion("Delete folder content"))
                    return;

                location.DeleteDirectoryContents();

                Logger.LogInformation("Folder content deleted: {folder}", location);

                await ShowMessageBoxAsync("Folder content deleted.", "Delete folder content");
            }
            catch (Exception e)
            {
                await SetErrorAsync(e, "Error in DeleteFolderContent");
            }
        }

        internal void SetSolutionFile(string solutionFile)
        {
            SolutionFile = solutionFile;
        }

        private TemplateOptions GetTemplateOptions()
        {
            return new TemplateOptions
            {
                SolutionFolder = Path.GetDirectoryName(SolutionFile),
                SolutionTemplate = SolutionTemplate,
                ProjectTemplates = ProjectItems.ConvertCheckedProjectTemplates(),
                TargetFolder = TemplateTargetFolder,
                AutoImportToVS = AutoImportToVS,
                ExcludedFolders = OptionsVM.ExcludedFolders,
                ExcludedFiles = OptionsVM.ExcludedFiles
            };
        }

        private void SetSolutionItems()
        {
            try
            {
                ProjectItems.Clear();
                List<IProjectTemplate> projectItems = new List<IProjectTemplate>(0);

                var defaultTemplateName = string.IsNullOrWhiteSpace(SolutionFile)
                    ? null
                    : Path.GetFileNameWithoutExtension(SolutionFile)
                        .Replace('.', ' ');

                if (!string.IsNullOrEmpty(defaultTemplateName))
                    SolutionTemplate.TemplateName = defaultTemplateName;
                SolutionTemplate.DefaultName = defaultTemplateName ?? SolutionTemplate.TemplateName;

                if (!SolutionFile.FileExists())
                {
                    return;
                }

                if (IsFileSolution)
                {
                    projectItems = _generatorService.GetProjectTemplates(SolutionFile, SolutionTemplate, OptionsVM.CopyPropertiesFromSolution);
                }
                else
                {
                    var solutionTemplate = _generatorService.ReadSolutionTemplate(SolutionFile);
                    projectItems.AddRange(solutionTemplate.Children);
                    solutionTemplate.Children.Clear();
                    SolutionTemplate = solutionTemplate.ToModel(null);
                }

                projectItems = projectItems.OrderBy(x => x.IsProject).ToList();

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

        private bool CanCancelOperation()
        {
            return CancelSource?.IsCancellationRequested == false;
        }

        private void CancelOperation()
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
                    //SetWarn(null, "Canceling...");
                    //OnCancelling();
                    CancelSource.Cancel();
                }
                catch (Exception e)
                {
                    SetError($"Error canceling: {e.Message}");
                }

                CloseWindowCommand.RaiseCanExecuteChanged();
            });
        }

        private void DisposeCancelSource()
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

        private void NewCancelSource()
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

                var errors = ValidateInputs();

                if (errors.Count != 0)
                {
                    var errorMessage = string.Join(Environment.NewLine, errors);
                    await ShowErrorAsync(errorMessage, "Validation");
                    return;
                }

                string existingContentMsg = string.Empty;
                if (TemplateTargetFolder.DirectoryExists())
                {
                    var dirCount = Directory.EnumerateDirectories(TemplateTargetFolder).Count();
                    CancelSource.Token.ThrowIfCancellationRequested();
                    var fileCount = Directory.EnumerateFiles(TemplateTargetFolder).Count();
                    CancelSource.Token.ThrowIfCancellationRequested();
                    if (dirCount != 0 || fileCount != 0)
                    {
                        existingContentMsg = "WARNING: Target folder already contains ";
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

                        existingContentMsg +=
                            $".{Environment.NewLine}Existing files will be overwritten!{Environment.NewLine}{Environment.NewLine}";
                    }
                }

                var options = GetTemplateOptions();
                var flattenedProjectTemplates = options.ProjectTemplates.GetTemplatesFlattened();
                var solutionFolderCount = flattenedProjectTemplates.Count(x => !x.IsProject);
                var projectCount = flattenedProjectTemplates.Count(x => x.IsProject);

                var confirmContentMsg =
                    $"{existingContentMsg}Are you sure you want to generate {projectCount} project templates";
                if (solutionFolderCount != 0)
                    confirmContentMsg += $" and {solutionFolderCount} solution folders";
                confirmContentMsg += "?";

                var msgBoxResult = await ShowMessageBoxAsync(confirmContentMsg,
                    "Generate Templates", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes;

                if (!msgBoxResult)
                {
                    return;
                }

                SaveSettings();

                var multiTemplateFile = new FileInfo(SolutionTemplateFullPath);
                if (multiTemplateFile.Exists)
                {
                    multiTemplateFile.Delete();
                }

                ShowBusyWindow();

                await Task.Run(() => { _generatorService.GenerateTemplate(options, CancelSource.Token); },
                    CancelSource.Token);

                CancelSource.Token.ThrowIfCancellationRequested();

                CloseBusyWindow();

                DisposeCancelSource();

                if (AutoOpenTemplate)
                {
                    try
                    {
                        Process.Start(multiTemplateFile.FullName);
                    }
                    catch (Exception e)
                    {
                        await SetErrorAsync(e, $"Error opening template: {e.Message}");
                    }
                }

                Logger.LogInformation($"Generated {projectCount} projects and {solutionFolderCount} successfully",
                    projectCount, solutionFolderCount);
                await ShowMessageBoxAsync($"Generated {projectCount} projects successfully!");
            }
            catch (OperationCanceledException)
            {
                CloseBusyWindow();
                Logger.LogWarning("Generating templates canceled!");

                await ShowMessageBoxAsync("Generating templates canceled!", null, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                CloseBusyWindow();
                await SetErrorAsync(ex, $"Error generating templates: {ex.Message}");
            }
            finally
            {
                DisposeCancelSource();
                IsBusy = false;
                CloseBusyWindow();
                RaisePropertyChanged(nameof(TemplateTargetFolder));
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
                DialogHost.Close(ViewNames.DialogRoot);
            }
            catch (Exception e)
            {
                SetError(e, $"Error in {nameof(CloseBusyWindow)}");
            }
        }

        private List<string> ValidateInputs()
        {
            var errors = SolutionTemplate.ValidateInputs();

            OutputPath = OutputPath.Trim();
            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                errors.Add(@"Output folder is missing.");
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

        public void AppClosing(CancelEventArgs e)
        {
            if (IsBusy)
            {
                if (!ShowQuestion($"{AppHelper.ProductName} is busy, are you sure you want to exit?", AppHelper.ProductName + " exit"))
                {
                    e.Cancel = true;
                    return;
                }
            }

            SaveSettings();
        }
    }
}
