using System.IO;
using System.Windows.Forms;
using MultiTemplateGeneratorLib.Generator;

namespace MultiTemplateGeneratorLib.Views
{
    public class MultiTemplateGeneratorViewModel
    {
        private readonly IMultiTemplateGeneratorService _multiTemplateGeneratorService;

        public MultiTemplateGeneratorViewModel(IMultiTemplateGeneratorService multiTemplateGeneratorService)
        {
            _multiTemplateGeneratorService = multiTemplateGeneratorService;

            Application.EnableVisualStyles();
        }

        public int OpenProjectSelector(string solutionFile)
        {
            string defaultTemplateName = string.IsNullOrWhiteSpace(solutionFile) ? null : Path.GetFileNameWithoutExtension(solutionFile);
            var solutionItems = _multiTemplateGeneratorService.GetSolutionFileItems(solutionFile);

            var projectSelector = new ProjectSelectorForm(_multiTemplateGeneratorService, solutionItems, true);

            projectSelector.SetTemplateOptions(defaultTemplateName);
            if (projectSelector.ShowDialog() != DialogResult.OK)
            {
                return 0;
            }

            Application.DoEvents();
            var selectedSolutionItems = projectSelector.SelectedSolutionItems;
            var options = projectSelector.GetTemplateOptions();

            return _multiTemplateGeneratorService.GenerateTemplate(solutionFile, options, selectedSolutionItems);
        }
    }
}
