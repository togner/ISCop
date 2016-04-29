using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowScriptStyleCop : ScriptTaskStyleCop
    {
        public DataflowScriptStyleCop(string styleCopSettingsPath)
            : base(styleCopSettingsPath)
        {
            this.ScriptFileName = "main.cs";
        }

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var pipe in package.GetControlFlowObjects<MainPipe>())
            {
                var mainPipe = (MainPipe)pipe.InnerObject;
                foreach (IDTSComponentMetaData100 comp in mainPipe.ComponentMetaDataCollection)
                {
                    var compInfo = ComponentInfo.Create(comp);
                    if (compInfo != null && compInfo.Name == ComponentInfo.ScriptName)
                    {
                        var language = comp.GetCustomPropertyValue<string>(CustomPropertyNames.ScriptLanguage);
                        if (language != ScriptTaskCSharp.CSharp)
                        {
                            continue;
                        }

                        var files = (string[])comp.CustomPropertyCollection[CustomPropertyNames.SourceCode].Value;
                        var mainFileIndex = Array.FindIndex(files, f => f == this.ScriptFileName);
                        var mainFileCode = files[mainFileIndex + 2];
                        this.Analyze(mainFileCode, Encoding.UTF8);

                        foreach (var violation in this.Violations)
                        {
                            var elementViolation = violation.Item1.Element.Violations.FirstOrDefault(v => v.Key == violation.Item1.Key);
                            var message = elementViolation != null ? elementViolation.Message : violation.Item1.Rule.Context;
                            var result = new Result(ResultType.Warning,
                                violation.Item1.Rule.CheckId,
                                violation.Item1.Rule.Name,
                                string.Format(CultureInfo.CurrentCulture, this.ResultMessageFormat, message, violation.Item2),
                                package.Name,
                                string.Format(CultureInfo.CurrentCulture, ScriptTaskStyleCop.ResultSourceFormat, comp.Name, pipe.Name),
                                violation.Item1.Line);
                            this.Results.Add(result);
                        }
                    }
                }
            }
        }
    }
}
