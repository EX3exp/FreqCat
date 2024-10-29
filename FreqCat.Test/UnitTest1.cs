using FreqCat.Utils;
using Serilog;
using System.Reflection;

namespace FreqCat.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestDirLoad()
        {
            string RootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string TestDir = Path.Combine(RootPath, "TestAssets", "NERo JA CVVC");
            DirectoryLoader dL = new DirectoryLoader(TestDir);
            Assert.AreEqual("NERo JA CVVC", dL.Data.RootName);
            Assert.AreEqual(TestDir, dL.Data.RootPath);
            Console.WriteLine(dL.Data.ToString());

        }
    }
}