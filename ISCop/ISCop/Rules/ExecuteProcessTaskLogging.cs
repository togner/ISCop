using System.Globalization;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ExecuteProcess;

namespace ISCop.Rules
{
    public class ExecuteProcessTaskLogging : PackageRule
    {
        public ExecuteProcessTaskLogging()
        {
            this.Id = "IS0006";
            this.Name = "ExecuteProcessTaskLogging";
            this.Description = "Every execute process task must log its output and error messages.";
            this.ResultMessageFormat = "Task \"{0}\" doesn't have StandardErrorVariable or StandardOutputVariable set. {1}";
        }

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var task in package.GetControlFlowObjects<ExecuteProcess>())
            {
                var execTask = (ExecuteProcess)task.InnerObject;
                if (string.IsNullOrEmpty(execTask.StandardErrorVariable)
                    || string.IsNullOrEmpty(execTask.StandardOutputVariable))
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, this.ResultMessageFormat, task.Name, this.Description);
                    this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, task.Name, -1));
                }
            }
        }
    }
}