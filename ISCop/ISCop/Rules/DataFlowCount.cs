using System.Collections.Generic;
using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowCount : PackageRule
    {
        public DataflowCount()
        {
            this.Id = "IS0105";
            this.Name = "DataflowCount";
            this.Description = "Consider using only one data flow per package to improve maintenance.";
            this.ResultMessageFormat = "There are {0} data flows in the package. {1}";
        }

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            List<TaskHost> pipelines = package.GetControlFlowObjects<MainPipe>();
            if (pipelines.Count > 1)
            {
                var msg = string.Format(CultureInfo.CurrentCulture, this.ResultMessageFormat, pipelines.Count, this.Description);
                this.Results.Add(new Result(ResultType.Information, this.Id, this.Name, msg, package.Name, null, -1));
            }
        }
    }
}
