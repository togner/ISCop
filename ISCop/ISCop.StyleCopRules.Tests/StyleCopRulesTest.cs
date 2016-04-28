using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleCop;

namespace ISCop.StyleCopRules.Tests
{
    [TestClass]
    public abstract class StyleCopRulesTest
    {
        private CodeProject _project;
        private StyleCopConsole StyleCop { get; set; }
        private List<string> Output { get; set; }
        private List<Violation> Violations { get; set; }

        protected StyleCopRulesTest()
        {
            string settings = Path.GetFullPath("Settings.StyleCop");
            this.StyleCop = new StyleCopConsole(settings, false, null, null, true);
            this.StyleCop.ViolationEncountered += ((sender, args) => this.Violations.Add(args.Violation));
            this.StyleCop.OutputGenerated += ((sender, args) => this.Output.Add(args.Output));
        }

        [TestInitialize]
        public void Setup()
        {
            this.Violations = new List<Violation>();
            this.Output = new List<string>();
            this._project = new CodeProject(Guid.NewGuid().GetHashCode(), "Stylecop.Settings", new Configuration(new string[0]));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this._project = null;
        }

        public void AddSourceCode(string fileName)
        {
            fileName = Path.GetFullPath(fileName);
            if (!File.Exists(fileName))
            {
                Assert.Fail("File {0} does not exist, test cannot proceed.", fileName);
            }
            this.StyleCop.Core.Environment.AddSourceCode(this._project, fileName, null);
        }

        public void StartAnalysis()
        {
            this.StyleCop.Start(new[] { this._project }, true);
        }

        public void AssertNotViolated(string ruleName)
        {
            if (this.Violations.Exists(x => x.Rule.Name == ruleName))
            {
                Assert.Fail("False positive for rule {0}.", ruleName);
            }
        }

        public void AssertViolated(string ruleName, params int[] lineNumbers)
        {
            if (lineNumbers != null && lineNumbers.Length > 0)
            {
                foreach (int lineNumber in lineNumbers)
                {
                    if (!this.Violations.Exists(x => x.Rule.Name == ruleName && x.Line == lineNumber))
                    {
                        Assert.Fail("Failed to violate rule {0} on line {1}. Violations count: {2}.", ruleName, lineNumber, this.Violations.Where((Violation v) => v.Rule.Name == ruleName).Count());
                    }
                }
            }
        }
    }
}
