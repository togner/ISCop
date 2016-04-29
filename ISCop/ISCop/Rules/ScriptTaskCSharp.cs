﻿using System.Globalization;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ScriptTask;
using Microsoft.SqlServer.VSTAHosting;

namespace ISCop.Rules
{
    public class ScriptTaskCSharp : PackageRule
    {
        protected const string CSharp = "CSharp";

        public ScriptTaskCSharp()
        {
            this.Id = "SSIS0001";
            this.Name = "ScriptTaskCSharp";
            this.Description = "Every script task must be written in C#.";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)")]
        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var scriptTask in package.GetControlFlowObjects<ScriptTask>())
            {
                var st = (ScriptTask)scriptTask.InnerObject;
                if (st.ScriptLanguage != VSTAScriptLanguages.GetDisplayName(ScriptTaskCSharp.CSharp))
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, "Script task {0} is written in {1}.", scriptTask.Name, st.ScriptLanguage);
                    this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, scriptTask.Name, -1));
                }
            }
        }
    }
}
