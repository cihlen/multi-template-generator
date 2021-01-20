using System.Collections;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiTemplateGenerator.Lib.Models;

namespace MultiTemplateGenerator.Lib.Tests.Generator
{
    internal class TestHelper
    {
        internal static readonly string OutputDir = Path.GetTempPath() + "VSSolutionTemplatesOutput";
        internal static readonly string TestSolutionFile = @"..\..\..\TestData\WebSolution\WebSolution.sln".GetAppPath();
        internal static readonly string TestSolutionTemplateFile = @"..\..\..\TestData\SolutionTemplates\WebSolution\WebSolution.vstemplate".GetAppPath();

        internal static void AssertProjectTemplate(IProjectTemplate expected, IProjectTemplate actual)
        {
            var properties = typeof(IProjectTemplate).GetProperties();

            foreach (var property in properties)
            {
                var expectedValue = property.GetValue(expected);
                if (expectedValue is IList list)
                    continue;

                var actualValue = property.GetValue(actual);

                if (actualValue != null && (property.Name.Equals("IconImagePath") || property.Name.Equals("PreviewImagePath")))
                {
                    actualValue = Path.GetExtension(actualValue.ToString());
                    if (expectedValue != null)
                        expectedValue = Path.GetExtension(expectedValue.ToString());
                }
                else if (actualValue != null && property.Name.Equals("ProjectFileName"))
                {
                    actualValue = Path.GetFileName(actualValue.ToString());
                    expectedValue = Path.GetFileName(expectedValue.ToString());
                }

                if (property.PropertyType == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)expectedValue))
                        expectedValue = string.Empty;
                    if (string.IsNullOrEmpty((string)actualValue))
                        actualValue = string.Empty;
                }

                Assert.AreEqual(expectedValue, actualValue, $"Property {property.Name} is not equal");
            }

            Assert.AreEqual(expected.Children.Count, actual.Children.Count);
            for (int i = 0; i < expected.Children.Count; i++)
            {
                var expectedItem = expected.Children[i];
                var actualItem = actual.Children[i];
                AssertProjectTemplate(expectedItem, actualItem);
            }
        }
    }
}
