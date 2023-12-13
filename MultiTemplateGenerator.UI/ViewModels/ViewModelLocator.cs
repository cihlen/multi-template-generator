using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
        private static bool _initialized;
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            var loggerFactory = LoggerHelper.GetLoggerFactory();

            var serviceCollection = new ServiceCollection()
               .AddSingleton(loggerFactory.CreateLogger<GeneratorViewModel>())
               .AddSingleton(loggerFactory.CreateLogger<TemplateGeneratorService>())
               .AddSingleton<GeneratorViewModel>()
               .AddSingleton<ITemplateGeneratorService, TemplateGeneratorService>()
               .AddSingleton<ITemplateRepository, TemplateRepository>()
               .AddSingleton<GeneratorViewModel>();

            Ioc.Default.ConfigureServices(serviceCollection
                .BuildServiceProvider());
        }

        public GeneratorViewModel MainVM => Ioc.Default.GetRequiredService<GeneratorViewModel>();
    }
}
