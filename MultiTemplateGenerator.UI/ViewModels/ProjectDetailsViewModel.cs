using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using MultiTemplateGenerator.UI.Models;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class ProjectDetailsViewModel : CommonViewModel
    {
        private RelayCommand _saveTemplatesCommand;
        private RelayCommand<string> _copyValuesCommand;

        public ProjectDetailsViewModel(ProjectTemplateModel model, ProjectTemplateModel solutionModel, ILogger logger)
            : base(logger)
        {
            Model = model.ToModel(model.Parent, null);
            SolutionModel = solutionModel;
        }

        public ProjectTemplateModel Model { get; }
        public ProjectTemplateModel SolutionModel { get; }

        public RelayCommand SaveTemplatesCommand => _saveTemplatesCommand ??= new RelayCommand(SaveTemplates, () => true);

        public RelayCommand<string> CopyValuesCommand => _copyValuesCommand ??= new RelayCommand<string>(CopyValues,
            (name) => !string.IsNullOrWhiteSpace(name));

        private void CopyValues(string name)
        {
            var parent = Model.GetParentProject() ?? SolutionModel;

            switch (name)
            {
                case "Description":
                    {
                        Model.Description = parent.Description;
                    }
                    break;
                case "PlatformTags":
                    {
                        Model.PlatformTags = parent.PlatformTags;
                    }
                    break;
                case "ProjectTypeTags":
                    {
                        Model.ProjectTypeTags = parent.ProjectTypeTags;
                    }
                    break;
                case "IconImagePath":
                    {
                        Model.IconImagePath = parent.IconImagePath;
                    }
                    break;
                case "PreviewImagePath":
                    {
                        Model.PreviewImagePath = parent.PreviewImagePath;
                    }
                    break;
            }
        }

        internal void SaveTemplates()
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
        }
    }
}
