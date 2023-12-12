using CommunityToolkit.Mvvm.ComponentModel;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class BusyViewModel : ObservableObject
    {
        public GeneratorViewModel GeneratorVM { get; }

        public BusyViewModel(GeneratorViewModel generatorViewModel)
        {
            GeneratorVM = generatorViewModel;
        }
    }
}
