using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using MultiTemplateGenerator.UI.Models;
using Serilog;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class ProjectDetailsViewModel : CommonViewModel
    {
        private RelayCommand _saveTemplatesCommand;

        public ProjectDetailsViewModel(ProjectTemplateModel model, ILogger logger)
            : base(logger)
        {
            Model = model;
        }

        public ProjectTemplateModel Model { get; }

        public RelayCommand SaveTemplatesCommand => _saveTemplatesCommand ??= new RelayCommand(SaveTemplates, () => true);

        internal void SaveTemplates()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
