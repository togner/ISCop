using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ISCop.StyleCopRules.Tests
{
    [TestClass]
    public class ISCopRulesTest : StyleCopRulesTest
    {
        [TestMethod]
        public void MainShouldHandleErrorsValidTest()
        {
            this.AddSourceCode("TestFiles\\Valid.cs");
            this.StartAnalysis();
            this.AssertNotViolated(ISCopRules.MainShouldHandleErrorsRuleName);
        }
    }
}
