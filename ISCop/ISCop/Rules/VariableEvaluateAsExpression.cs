using System.Globalization;
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

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            this.ProcessObject(package.Name, package, string.Empty);
        }

        private void ProcessObject(string packageName, object component, string path)
        {
            DtsContainer container = component as DtsContainer;

            // Should only get package as we call GetPackage up front. Could make scope like, but need UI indicator that this is happening
            Package package = component as Package;
            if (package != null)
            {
                path = "\\Package";
            }
            else if (!(component is DtsEventHandler))
            {
                path = path + "\\" + container.Name;
            }
            if (container != null)
            {
                this.ScanVariables(packageName, path, container.Variables);
            }
            EventsProvider eventsProvider = component as EventsProvider;
            if (eventsProvider != null)
            {
                foreach (DtsEventHandler eventhandler in eventsProvider.EventHandlers)
                {
                    this.ProcessObject(packageName, eventhandler, path + ".EventHandlers[" + eventhandler.Name + "]");
                }
            }
            IDTSSequence sequence = component as IDTSSequence;
            if (sequence != null)
            {
                this.ProcessSequence(packageName, sequence, path);
            }
        }

        private void ProcessSequence(string packageName, IDTSSequence sequence, string path)
        {
            foreach (Executable executable in sequence.Executables)
            {
                this.ProcessObject(packageName, executable, path);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EvaluateAsExpression")]
        private void ScanVariables(string packageName, string objectPath, Variables variables)
        {
            foreach (Variable variable in variables)
            {
                if (string.IsNullOrEmpty(variable.Expression))
                {
                    continue;
                }
                if (!variable.EvaluateAsExpression)
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, "Variable \"{0}\" has an Expression set, but the EvaluateAsExpression property is false. The variable value will be static and the expression will not be used. Consider removing the expression or setting EvaluateAsExpression to true. Path to variable {1}.", variable.Name, objectPath);
                    this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, packageName, objectPath, -1));
                }
            }
        }
    }
}

