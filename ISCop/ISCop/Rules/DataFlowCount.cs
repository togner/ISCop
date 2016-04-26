using System.Collections.Generic;
using BIDSHelper.SSIS;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataFlowCount : PackageRule
    {
        public DataFlowCount()
        {
            this.Id = "BIDS0002";
            this.Name = "DataFlowCount";
            this.Description = "Checks for the number of data flows in the package";
        }

        public override void Check(Package package)
        {
            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);
            if (pipelines.Count > 1)
            {
                var msg = "There are " + pipelines.Count + " data flows in the package. For simplicity, encapsulation, and to facilitate team development, consider using only one data flow per package.";
                this.Results.Add(new Result(ResultType.Information, this.Id, this.Name, msg, package.Name, null, -1));
            }
        }
    }
}
