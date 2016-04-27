using System.Globalization;
using BIDSHelper.SSIS;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class VariableEvaluateAsExpression : PackageRule
    {
        public VariableEvaluateAsExpression()
        {
            this.Id = "BIDS0006";
            this.Name = "VariableEvaluateAsExpression";
            this.Description = "Validates that any variable which has an expression set also has the EvaluateAsExpression property set to true.";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EvaluateAsExpression")]
        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var task in PackageHelper.GetControlFlowObjects<object>(package))
            {
                var mainTask = task as DtsContainer;
                if (mainTask != null)
                {
                    foreach (var variable in mainTask.Variables)
                    {
                        if (!string.IsNullOrEmpty(variable.Expression) && !variable.EvaluateAsExpression)
                        {
                            var msg = string.Format(CultureInfo.CurrentCulture, "Variable \"{0}\" has an Expression set, but the EvaluateAsExpression property is false. The variable value will be static and the expression will not be used. Consider removing the expression or setting EvaluateAsExpression to true.", variable.Name);
                            this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                        }
                    }
                }
            }
        }
    }
}

