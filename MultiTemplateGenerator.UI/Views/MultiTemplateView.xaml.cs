using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using FontAwesome.WPF;
using MaterialDesignThemes.Wpf;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.UI.Helpers;
using MultiTemplateGenerator.UI.ViewModels;

namespace MultiTemplateGenerator.UI.Views
{
    /// <summary>
    /// Interaction logic for MultiTemplateView.xaml
    /// </summary>
    public partial class MultiTemplateView
    {
        public MultiTemplateView()
        {
            try
            {
                ShadowAssist.SetShadowDepth(this, ShadowDepth.Depth0);
                FontAwesome.WPF.ImageAwesome im = new ImageAwesome();

                if (Application.Current == null)
                {
                    // create the Application object
                    // ReSharper disable once ObjectCreationAsStatement
                    new Application();
                }
                var resourceXmlFiles = new List<string>
                {
                    "/MultiTemplateGenerator.UI;component/Resources/ViewResources.xaml",
                    "/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml",
                    "/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml",
                    "/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml"
                };
                // merge in your application resources
                foreach (var resourceXmlFile in resourceXmlFiles)
                {
                    Application.Current?.Resources.MergedDictionaries.Add(
                        Application.LoadComponent(new Uri(resourceXmlFile, UriKind.Relative)) as ResourceDictionary);
                }
                InitializeComponent();

                this.Icon = Properties.Resources.MultiTemplateGenerator.ToImageSource();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                e.ShowErrorMessageBox();
                throw;
            }
        }

        public MultiTemplateView(string solutionFilePath)
            : this()
        {
            try
            {
                var viewModel = ((ViewModelLocator)FindResource("Locator")).MainVM;

                viewModel.SetSolutionFile(solutionFilePath);
                DataContext = viewModel;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                e.ShowErrorMessageBox();
                throw;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var viewModel = DataContext as GeneratorViewModel;
                if (viewModel == null)
                    return;

                viewModel.AppClosing(e);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
                exception.ShowErrorMessageBox();
            }
        }

        private void MenuItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as GeneratorViewModel;
            if (viewModel == null)
                return;

            viewModel.IsAppMenuOpen = false;
        }
    }
}
