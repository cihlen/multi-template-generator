using GalaSoft.MvvmLight;

namespace MultiTemplateGenerator.UI.ViewModels
{
    public class BusyViewModel : ViewModelBase
    {
        public GeneratorViewModel GeneratorVM { get; }

        public BusyViewModel(GeneratorViewModel generatorViewModel)
        {
            GeneratorVM = generatorViewModel;
        }
    }
}
