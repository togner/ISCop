using System.Globalization;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class VariableEvaluateAsExpression : PackageRule
    {
        public VariableEvaluateAsExpression()
        {
            this.Id = "IS0005";
            this.Name = "VariableEvaluateAsExpression";
            this.Description = "Variable which has an expression set should also have EvaluateAsExpression property set to true. Otherwise the variable value will be static and the expression will not be used.";
            this.ResultMessageFormat = "Variable \"{0}\" has an expression set, but EvaluateAsExpression property is false. {1}";
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
                    foreach (var variable in mainTask.Variables)
                    {
                        if (!string.IsNullOrEmpty(variable.Expression) && !variable.EvaluateAsExpression)
                        {
                            var msg = string.Format(CultureInfo.CurrentCulture, this.ResultMessageFormat, variable.Name, this.Description);
                            this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, mainTask.Name, -1));
                        }
                    }
                }
            }
        }
    }
}