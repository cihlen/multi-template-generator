using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Extensions.Logging;
using MultiTemplateGenerator.Lib.Generator;
using MultiTemplateGenerator.UI.Helpers;

namespace MultiTemplateGenerator.UI.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (!SimpleIoc.Default.IsRegistered<ILogger<GeneratorViewModel>>())
            {
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddEventLog(settings =>
                    {
                        settings.LogName = "Application";
                    });
                });

                ILogger<GeneratorViewModel> generatorVMLogger = loggerFactory.CreateLogger<GeneratorViewModel>();

                SimpleIoc.Default.Register(() => loggerFactory.CreateLogger<GeneratorViewModel>());
                SimpleIoc.Default.Register(() => loggerFactory.CreateLogger<TemplateGeneratorService>());
            }

            SimpleIoc.Default.Register<ITemplateRepository, TemplateRepository>();
            SimpleIoc.Default.Register<ITemplateGeneratorService, TemplateGeneratorService>();
            SimpleIoc.Default.Register<GeneratorViewModel>();
        }

        public GeneratorViewModel MainVM => ServiceLocator.Current.GetInstance<GeneratorViewModel>();
    }
}
