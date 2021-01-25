using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MultiTemplateGenerator.Lib.Generator;
using MultiTemplateGenerator.Lib.Models;

namespace MultiTemplateGenerator.Lib.Tests.Generator
{
    [TestClass()]
    public class TemplateGeneratorServiceTests
    {
        private readonly string _outputDir = TestHelper.OutputDir;
        private readonly string _testSolutionFile = TestHelper.TestSolutionFile;
        private readonly ILogger<TemplateGeneratorService> _logger = new Mock<ILogger<TemplateGeneratorService>>().Object;

        [TestInitialize]
        public void TestInit()
        {
            _outputDir.RecreateDirectory();
        }

        [TestMethod()]
        public void GenerateSolutionTemplate_FromSolution()
        {
            var templateGeneratorWriter = new TemplateRepository();
            TemplateGeneratorService generator = new TemplateGeneratorService(templateGeneratorWriter, _logger);

            IProjectTemplate solutionTemplate = new ProjectTemplate(true)
            {
                TemplateName = "GeneratedSolutionTemplate"
            };

            var projectItems = generator.GetProjectTemplates(_testSolutionFile, solutionTemplate, true);

            var templateOptions = new TemplateOptions
            {
                SolutionFolder = Path.GetDirectoryName(_testSolutionFile),
                TargetFolder = _outputDir,
                SolutionTemplate = solutionTemplate,
                ProjectTemplates = projectItems
            };

            generator.GenerateTemplate(templateOptions, CancellationToken.None);

            var solutionTemplateLines = File.ReadAllLines(templateOptions.TargetTemplatePath);

            Assert.AreEqual(66, solutionTemplateLines.Length);
        }

        [TestMethod()]
        public void GenerateSolutionTemplate_FromSolutionTemplate()
        {
            TemplateGeneratorService generator = new TemplateGeneratorService(new TemplateRepository(), _logger);
            var templateFile = new FileInfo(TestHelper.TestSolutionTemplateFile);
            Assert.IsTrue(templateFile.Exists);

            IProjectTemplate solutionTemplate = generator.ReadSolutionTemplate(templateFile.FullName);

            var templateOptions = new TemplateOptions
            {
                TargetFolder = _outputDir,
                SolutionTemplate = solutionTemplate,
                ProjectTemplates = solutionTemplate.Children,
                SolutionFolder = templateFile.DirectoryName
            };

            generator.GenerateTemplate(templateOptions, CancellationToken.None);

            IProjectTemplate generatedTemplate = generator.ReadSolutionTemplate(templateOptions.TargetTemplatePath);

            TestHelper.AssertProjectTemplate(solutionTemplate, generatedTemplate);
        }
    }
}