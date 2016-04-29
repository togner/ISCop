using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ScriptTask;
using Microsoft.SqlServer.VSTAHosting;
using StyleCop;

namespace ISCop.Rules
{
    /// <summary>
    /// Runs StyleCop on C# script tasks.
    /// Path to settings file can be passed as param but parser and rule DLLs must be in root folder.
    /// </summary>
    public class ScriptTaskStyleCop : ScriptTaskCSharp
    {
        protected const string ResultSourceFormat = "{0} ({1})";
        protected string ScriptFileName { get; set; } 
        protected StyleCopConsole StyleCop { get; private set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected List<Tuple<Violation, string>> Violations { get; private set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        protected List<string> Output { get; private set; }

        public ScriptTaskStyleCop(string styleCopSettingsPath)
        {
            this.ResultMessageFormat = "{0} To suppress: {1}";
            this.ScriptFileName = "ScriptMain.cs";
            if (!File.Exists(styleCopSettingsPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "StyleCop settings file '{0}' doesn't exist.", styleCopSettingsPath));
            }
            this.StyleCop = new StyleCopConsole(styleCopSettingsPath, false, null, null, true);
            this.Violations = new List<Tuple<Violation, string>>();
            this.Output = new List<string>();
            this.StyleCop.ViolationEncountered += ((sender, a) => this.Violations.Add(new Tuple<Violation, string>(a.Violation, "[SuppressMessage(\"" + a.Violation.Rule.Namespace + "\", \"" + a.Violation.Rule.CheckId + ":" + a.Violation.Rule.Name + "\")]")));
            this.StyleCop.OutputGenerated += ((sender, a) => this.Output.Add(a.Output));
        }

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var scriptTask in package.GetControlFlowObjects<ScriptTask>())
            {
                var st = (ScriptTask)scriptTask.InnerObject;
                if (st.ScriptLanguage != VSTAScriptLanguages.GetDisplayName(ScriptTaskStyleCop.CSharp))
                {
                    continue;
                }
                var vstaFile = (VSTAScriptProjectStorage.VSTAScriptFile)(st.ScriptStorage.ScriptFiles[this.ScriptFileName]);
                this.Analyze(vstaFile.Data, Encoding.UTF8);
                foreach (var violation in this.Violations)
                {
                    var elementViolation = violation.Item1.Element.Violations.FirstOrDefault(v => v.Key == violation.Item1.Key);
                    var message = elementViolation != null ? elementViolation.Message : violation.Item1.Rule.Context;
                    var result = new Result(ResultType.Warning,
                        violation.Item1.Rule.CheckId,
                        violation.Item1.Rule.Name,
                        string.Format(CultureInfo.CurrentCulture, this.ResultMessageFormat, message, violation.Item2),
                        package.Name,
                        string.Format(CultureInfo.CurrentCulture, ScriptTaskStyleCop.ResultSourceFormat, st.ScriptProjectName, scriptTask.Name),
                        violation.Item1.Line);
                    this.Results.Add(result);
                }
            }
        }

        protected void Analyze(string script, Encoding encoding)
        {
            string tempFile = null;
            try
            {
                this.Violations.Clear();
                this.Output.Clear();

                tempFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";
                File.WriteAllText(tempFile, script, encoding);
                var sproject = this.CreateSingleFileProject(tempFile);
                this.StyleCop.Start(new List<CodeProject> { sproject }, true);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private CodeProject CreateSingleFileProject(string filePath)
        {
            var sproject = new CodeProject(Guid.NewGuid().GetHashCode(), "Stylecop.Settings", new StyleCop.Configuration(new string[0]));
            this.StyleCop.Core.Environment.AddSourceCode(sproject, filePath, null);
            return sproject;
        }
    }
}
