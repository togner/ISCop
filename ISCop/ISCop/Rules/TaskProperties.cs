using System.Globalization;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class TaskProperties : PackageRule
    {
        private const string ForceExecutionResultResultMessageFormat = "Task {0} should have ForceExecutionResult=None but it's {1}. {2}";
        private const string FailPackageOnFailureResultMessageFormat = "Task {0} should have FailPackageOnFailure set to true. {1}";
        private const string FailParentOnFailureResultMessageFormat = "Task {0} should have FailParentOnFailure set to true. {1}";
        public TaskProperties()
        {
            this.Id = "IS0003";
            this.Name = "TaskProperties";
            this.Description = "FailParentOnFailure and FailPackageOnFailure must be set to true, ForceExecutionResult must be set to None.";
        }

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var task in package.GetControlFlowObjects<object>())
            {
                var mainTask = task as DtsContainer;
                if (mainTask != null)
                {
                    if (mainTask.ForceExecutionResult != DTSForcedExecResult.None)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, TaskProperties.ForceExecutionResultResultMessageFormat, mainTask.Name, mainTask.ForceExecutionResult, this.Description);
                        this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                    }
                    if (!mainTask.FailPackageOnFailure)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, TaskProperties.FailPackageOnFailureResultMessageFormat, mainTask.Name, this.Description);
                        this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                    }
                    if (!mainTask.FailParentOnFailure)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, TaskProperties.FailParentOnFailureResultMessageFormat, mainTask.Name, this.Description);
                        this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                    }
                }
            }
        }
    }
}

