using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowCount : PackageRule
    {
        public DataflowCount()
        {
            this.Id = "BIDS0002";
            this.Name = "DataFlowCount";
            this.Description = "Checks for the number of data flows in the package";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)")]
        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            List<TaskHost> pipelines = package.GetControlFlowObjects<MainPipe>();
            if (pipelines.Count > 1)
            {
                var msg = "There are " + pipelines.Count + " data flows in the package. For simplicity, encapsulation, and to facilitate team development, consider using only one data flow per package.";
                this.Results.Add(new Result(ResultType.Information, this.Id, this.Name, msg, package.Name, null, -1));
            }
        }
    }
}
