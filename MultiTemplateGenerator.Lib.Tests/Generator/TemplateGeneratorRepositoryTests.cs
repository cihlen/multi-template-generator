using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiTemplateGenerator.Lib.Generator;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.Lib.SolutionParser;

namespace MultiTemplateGenerator.Lib.Tests.Generator
{
    [TestClass()]
    public class TemplateGeneratorRepositoryTests
    {
        private readonly string _outputDir = TestHelper.OutputDir;
        private readonly string _testSolutionFile = TestHelper.TestSolutionFile;

        [TestInitialize]
        public void TestInit()
        {
            _outputDir.RecreateDirectory();
        }

        [TestMethod()]
        public void ParseSolutionTest()
        {
            var solution = SolutionFileParser.ParseSolutionFile(_testSolutionFile);
            var solutionItems = solution.GetSortedHierarchy();

            Assert.AreEqual(17, solution.Count);
            Assert.AreEqual(4, solutionItems.Count);

            Assert.AreEqual(0, solutionItems[0].Children.Count);
            Assert.AreEqual(4, solutionItems[1].Children.Count);
            Assert.AreEqual(2, solutionItems[2].Children.Count);
            Assert.AreEqual(4, solutionItems[3].Children.Count);

            Assert.AreEqual(1, solutionItems[1].Children[1].Children.Count);
            Assert.AreEqual(2, solutionItems[1].Children[2].Children.Count);
        }

        [TestMethod()]
        public void ReadSolutionTemplate_ShouldReadProjectTemplate()
        {
            ITemplateRepository templateGeneratorWriter = new TemplateRepository();
            var templateFile = new FileInfo(TestHelper.TestSolutionTemplateFile);
            Assert.IsTrue(templateFile.Exists);

            IProjectTemplate solutionTemplate = templateGeneratorWriter.ReadSolutionTemplate(templateFile.FullName);

            var allTemplates = solutionTemplate.Children.GetTemplatesFlattened();
            Assert.AreEqual(17, allTemplates.Count);
        }

        [TestMethod()]
        public void ReadProjectTemplate_ShouldReadProjectTemplate()
        {
            ITemplateRepository templateGeneratorWriter = new TemplateRepository();
            var templateFile = new FileInfo(@"..\..\..\TestData\Templates\RazorClassLibrary\RazorClassLibrary.vstemplate".GetAppPath());
            Assert.IsTrue(templateFile.Exists);

            IProjectTemplate projectTemplate = templateGeneratorWriter.ReadProjectTemplate(templateFile.FullName);

            Assert.AreEqual("RazorClassLibrary\\RazorClassLibrary1.csproj", projectTemplate.ProjectFileName);
            Assert.AreEqual(templateFile.Name, projectTemplate.TemplateFileName);

            Assert.AreEqual("RazorClassLibrary", projectTemplate.TemplateName);
            Assert.AreEqual("<No description available>", projectTemplate.Description);
            Assert.AreEqual("CSharp", projectTemplate.ProjectType);
            Assert.AreEqual("C#", projectTemplate.LanguageTag);
            Assert.AreEqual("", projectTemplate.PlatformTags);
            Assert.AreEqual("", projectTemplate.ProjectTypeTags);
            Assert.AreEqual("", projectTemplate.ProjectSubType);
            Assert.AreEqual(1000, projectTemplate.SortOrder);
            Assert.AreEqual(true, projectTemplate.CreateNewFolder);
            Assert.AreEqual("RazorClassLibrary", projectTemplate.DefaultName);
            Assert.AreEqual(true, projectTemplate.ProvideDefaultName);
            Assert.AreEqual(LocationFieldType.Enabled, projectTemplate.LocationField);
            Assert.AreEqual(true, projectTemplate.EnableLocationBrowseButton);
            Assert.AreEqual(true, projectTemplate.CreateInPlace);
            Assert.AreEqual("__TemplateIcon.ico", Path.GetFileName(projectTemplate.IconImagePath));
            Assert.AreEqual(null, projectTemplate.PreviewImagePath);
            Assert.AreEqual(false, projectTemplate.IsHidden);
            Assert.AreEqual(null, projectTemplate.MaxFrameworkVersion);
            Assert.AreEqual(null, projectTemplate.RequiredFrameworkVersion);
            Assert.AreEqual(null, projectTemplate.FrameworkVersion);
        }

        [TestMethod()]
        public void CreateProjectTemplate_ShouldBeSame()
        {
            ITemplateRepository templateGeneratorWriter = new TemplateRepository();

            var templateFile = new FileInfo(@"..\..\..\TestData\Templates\RazorClassLibrary\RazorClassLibrary.vstemplate".GetAppPath());
            Assert.IsTrue(templateFile.Exists);

            IProjectTemplate projectTemplate = templateGeneratorWriter.ReadProjectTemplate(templateFile.FullName);

            templateGeneratorWriter.CreateProjectTemplate(projectTemplate, projectTemplate.DefaultName, templateFile.Directory.Parent.FullName, _outputDir, false, string.Empty, string.Empty, CancellationToken.None);

            var generatedTemplateFile = new FileInfo(projectTemplate.GetTemplateFileName(_outputDir));
            IProjectTemplate projectTemplateWritten = templateGeneratorWriter.ReadProjectTemplate(generatedTemplateFile.FullName);

            TestHelper.AssertProjectTemplate(projectTemplate, projectTemplateWritten);
        }

        [TestMethod()]
        public void ReplaceFileWithTemplateParameters()
        {
            string testCodeFile = @"..\..\..\TestData\SolutionTemplates\WebSolution\WebApplicationTest\Startup.cs".GetAppPath();
            var tempCodeFile = Path.GetTempFileName();

            ITemplateRepository templateRepository = new TemplateRepository();

            var changes = templateRepository.ReplaceWithTemplateParameters(testCodeFile, "WebApplicationTest", tempCodeFile);

            Assert.AreEqual(1, changes);
        }


        [TestMethod()]
        public void CreateSolutionTemplateTest()
        {
            ITemplateRepository templateRepository = new TemplateRepository();

            var solution = SolutionFileParser.ParseSolutionFile(_testSolutionFile);
            var solutionFileItems = solution.GetSortedHierarchy();
            var projectTemplates = new List<IProjectTemplate>();

            var solutionTemplate = new ProjectTemplate(true)
            {
                TemplateName = "TestSolutionTemplate",
                Description = "DEscr",
                LanguageTag = "C#",
                CreateNewFolder = true
            };

            foreach (var solutionItem in solutionFileItems)
            {
                projectTemplates.Add(templateRepository.ReadProjectTemplate(Path.GetDirectoryName(_testSolutionFile), solutionItem, solutionTemplate, true));
            }

            var solutionTemplateFileName = Path.Combine(_outputDir, "solution.vstemplate");

            templateRepository.CreateSolutionTemplate(solutionTemplateFileName, solutionTemplate, projectTemplates);
        }




        //private void AssertXmlProjectItems(IEnumerable<XmlTemplateItemBase> expected, IEnumerable<XmlTemplateItemBase> actual)
        //{
        //    Assert.AreEqual(expected.Count(), actual.Count());

        //    var expectedFolders = expected.GetFolders().ToList();
        //    var actualFolders = actual.GetFolders().ToList();
        //    Assert.AreEqual(expectedFolders.Count, actualFolders.Count);
        //    for (var i = 0; i < expectedFolders.Count; i++)
        //    {
        //        var expectedItem = expectedFolders[i];
        //        var actualItem = actualFolders[i];
        //        Assert.AreEqual(expectedItem.Name, actualItem.Name);
        //        Assert.AreEqual(expectedItem.TargetFolderName, actualItem.TargetFolderName);
        //        AssertXmlProjectItems(expectedItem.Children, actualItem.Children);
        //    }

        //    var expectedItems = expected.GetProjectItems().ToList();
        //    var actualItems = actual.GetProjectItems().ToList();
        //    Assert.AreEqual(expectedItems.Count, actualItems.Count);

        //    for (int i = 0; i < expectedItems.Count; i++)
        //    {
        //        var expectedItem = expectedItems[i];
        //        var actualItem = actualItems[i];
        //        Assert.AreEqual(expectedItem.Content, actualItem.Content);
        //        Assert.AreEqual(expectedItem.OpenInWebBrowser, actualItem.OpenInWebBrowser);
        //        Assert.AreEqual(expectedItem.OpenOrder, actualItem.OpenOrder);
        //        Assert.AreEqual(expectedItem.OpenInEditor, actualItem.OpenInEditor);
        //        Assert.AreEqual(expectedItem.OpenInHelpBrowser, actualItem.OpenInHelpBrowser);
        //        Assert.AreEqual(expectedItem.ReplaceParameters, actualItem.ReplaceParameters);
        //        Assert.AreEqual(expectedItem.TargetFileName, actualItem.TargetFileName);
        //    }
        //}
    }
}