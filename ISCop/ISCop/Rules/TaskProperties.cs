using System.Globalization;
using BIDSHelper.SSIS;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class TaskProperties : PackageRule
    {
        public TaskProperties()
        {
            this.Id = "SSIS0004";
            this.Name = "TaskProperties";
            this.Description = "FailParentOnFailure and FailPackageOnFailure must be set to true, ForceExecutionResult must be set to None.";
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
                    if (mainTask.ForceExecutionResult != DTSForcedExecResult.None)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "Task {0} should have ForceExecutionResult=None but it's {1}", mainTask.Name, mainTask.ForceExecutionResult);
                        this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                    }
                    if (!mainTask.FailPackageOnFailure)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "Task {0} should have FailPackageOnFailure set to true.", mainTask.Name);
                        this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                    }
                    if (!mainTask.FailParentOnFailure)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "Task {0} should have FailParentOnFailure set to true.", mainTask.Name);
                        this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                    }
                }
            }
        }
    }
}

