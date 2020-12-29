using System;
using System.Linq;
using System.Windows.Forms;
using MultiTemplateGeneratorLib;
using MultiTemplateGeneratorLib.Extensions;
using MultiTemplateGeneratorLib.Generator;
using MultiTemplateGeneratorLib.Views;

namespace TestConsole
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            var testSolutionName = @"..\..\..\TestData\WebSolution\WebSolution.sln".GetAppFile();

            if (args.Length == 1)
            {
                testSolutionName = args[0];
            }

            MultiTemplateGeneratorViewModel vm = new MultiTemplateGeneratorViewModel(new MultiTemplateGeneratorService(new TemplateGeneratorWriter()));

            var generatedCount = vm.OpenProjectSelector(testSolutionName);
            if (generatedCount != 0)
            {
                MessageBox.Show($"Generated {generatedCount} templates.", "Multi-Template Generator Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return generatedCount != 0 ? 0 : 1;
        }
    }
}
