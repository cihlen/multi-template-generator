
using System.Windows;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.UI.Views;

namespace MultiTemplateGenerator.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string testSolutionName = null;

            if (e.Args.Length > 0)
            {
                testSolutionName = e.Args[0];
                if (!testSolutionName.FileExists())
                    testSolutionName = null;
            }

            MultiTemplateView projectSelectView = new MultiTemplateView(testSolutionName)
            {
                ShowInTaskbar = true
            };
            projectSelectView.ShowDialog();
        }
    }
}
