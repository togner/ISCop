using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BIDSHelper.SSIS;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ScriptTask;
using Microsoft.SqlServer.VSTAHosting;
using StyleCop;

namespace ISCop
{
    /// <summary>
    /// Runs StyleCop on C# script tasks.
    /// Path to settings file can be passed as param but parser and rule DLLs must be in root folder.
    /// </summary>
    public class ScriptTaskStyleCop : ScriptTaskCSharp
    {
        private const string ScriptFileName = "ScriptMain.cs";
        private const VSTAScriptProjectStorage.Encoding ScriptEncoding = VSTAScriptProjectStorage.Encoding.UTF8;
        protected StyleCopConsole StyleCop { get; private set; } 
        private List<Tuple<Violation, string>> Violations { get; set; }
        private List<string> Output { get; set; }

        public ScriptTaskStyleCop(string styleCopSettingsPath)
        {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)")]
        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var scriptTask in PackageHelper.GetControlFlowObjects<ScriptTask>(package))
            {
                this.Violations.Clear();
                this.Output.Clear();
                var st = (ScriptTask)scriptTask.InnerObject;
                if (st.ScriptLanguage != VSTAScriptLanguages.GetDisplayName(ScriptTaskStyleCop.ScriptLanguage))
                {
                    continue;
                }
                string tempFile = null;
                try
                {
                    tempFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cs";

                    var vstaFile = (VSTAScriptProjectStorage.VSTAScriptFile)(st.ScriptStorage.ScriptFiles[ScriptTaskStyleCop.ScriptFileName]);
                    ////if (vstaFile.Encoding == StyleCopPackageRule.ScriptEncoding) 
                    File.WriteAllText(tempFile, vstaFile.Data, Encoding.UTF8);

                    var sconfiguration = new StyleCop.Configuration(new string[0]);
                    var sproject = new CodeProject(Guid.NewGuid().GetHashCode(), "Stylecop.Settings", sconfiguration);

                    this.StyleCop.Core.Environment.AddSourceCode(sproject, tempFile, null);
                    this.StyleCop.Start(new List<CodeProject> { sproject }, true);
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }

                foreach (var violation in this.Violations)
                {
                    var elementViolation = violation.Item1.Element.Violations.FirstOrDefault(v => v.Key == violation.Item1.Key);
                    var message = elementViolation != null ? elementViolation.Message : violation.Item1.Rule.Context;
                    var result = new Result(ResultType.Warning,
                        violation.Item1.Rule.CheckId,
                        violation.Item1.Rule.Name,
                        message + " To suppress: " + violation.Item2,
                        package.Name,
                        string.Format(CultureInfo.CurrentCulture, "{0} ({1})", st.ScriptProjectName, scriptTask.Name),
                        violation.Item1.Line);
                    this.Results.Add(result);
                }
            }
        }
    }
}
