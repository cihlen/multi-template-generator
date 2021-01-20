using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MultiTemplateGenerator.Lib.Tests.Generator
{
    [TestClass]
    public class CommonUnitTests
    {
        [TestMethod]
        public void WildcardComparer_TestEquals()
        {
            WildcardComparer wildcardComparer = new WildcardComparer();
            
            var result = wildcardComparer.Equals(".*", ".vse");

            Assert.IsTrue(result);

            result = wildcardComparer.Equals(".dd", ".vse");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WildcardComparer_TestContains()
        {
            WildcardComparer wildcardComparer = new WildcardComparer();

            var blackList = new List<string> { "bin", "obj", "TestResults", ".*" };
            var testList = new List<string> { "bin", "bin wefwefw", "wefssswefw obj", ".vs", "v.s", ".rgtr", "rferg obj we" };

            var result = testList.Where(x => !blackList.Contains(x, wildcardComparer)).ToList();

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("bin wefwefw", result[0]);
            Assert.AreEqual("wefssswefw obj", result[1]);
            Assert.AreEqual("v.s", result[2]);
            Assert.AreEqual("rferg obj we", result[3]);
        }
    }
}
