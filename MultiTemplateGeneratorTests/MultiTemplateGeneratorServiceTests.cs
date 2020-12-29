using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MultiTemplateGeneratorLib;
using MultiTemplateGeneratorLib.Extensions;
using MultiTemplateGeneratorLib.Generator;

namespace MultiTemplateGeneratorTests
{
    [TestClass()]
    public class MultiTemplateGeneratorServiceTests
    {
        [TestMethod()]
        public void ParseSolutionTest()
        {
            //var templateGeneratorWriterMock = new Mock<ITemplateGeneratorWriter>();
            //MultiTemplateGeneratorService service = new MultiTemplateGeneratorService(templateGeneratorWriterMock.Object);

            var testSolutionFile = @"..\..\TestData\TestData\WebSolution\WebSolution.sln".GetAppFile();
            var solution = SolutionFileParser.ParseSolutionFile(testSolutionFile);
            var solutionItems = solution.GetSortedHierarchy();

            Assert.AreEqual(2, solutionItems.Count);
        }
    }
}