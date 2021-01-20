using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MultiTemplateGenerator.Lib.Tests.Generator
{
    [TestClass]
    public class TemplateExtensionsTests
    {
        [TestMethod]
        public void GetProjectName_ShouldBeChanged()
        {
            var projectName = "MyProject Template.App".GetProjectName("MyProject Template");
            Assert.AreEqual("$safeprojectname$.App", projectName);
            
            projectName = "MyProject Template.App".GetProjectName("MyProject Template 2");
            Assert.AreEqual("MyProject Template.App", projectName);

            projectName = "MyProject Template App".GetProjectName("MyProject Template");
            Assert.AreEqual("$safeprojectname$ App", projectName);

            projectName = "MyProject Template App".GetProjectName("MyProject Template 2");
            Assert.AreEqual("MyProject Template App", projectName);
        }

        [TestMethod]
        public void GetProjectName_ShouldNotBeChanged()
        {
            var projectName = "MyProject2 Template.App".GetProjectName("MyProject Template");
            Assert.AreEqual("MyProject2 Template.App", projectName);
        }
    }
}
