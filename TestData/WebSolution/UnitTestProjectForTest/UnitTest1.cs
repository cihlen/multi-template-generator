using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplicationTest;

namespace UnitTestProjectForTest
{
    [TestClass]
    public class UnitTestForTest
    {
        [TestMethod]
        public void TestMethodForTest()
        {
            var startup = new Startup(null);
            Assert.IsNotNull(startup);
        }
    }
}
