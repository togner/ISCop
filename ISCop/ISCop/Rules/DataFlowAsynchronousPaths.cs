using System.Collections.Generic;
using System.Globalization;
using BIDSHelper.SSIS;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowAsynchronousPaths : PackageRule
    {
        public DataflowAsynchronousPaths()
        {
            this.Id = "BIDS0001";
            this.Name = "DataFlowAsynchronousPaths";
            this.Description = "Checks for asynchronous paths in the data flow";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)")]
        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);
            foreach (var pipe in pipelines)
            {
                var mainPipe = (MainPipe)pipe.InnerObject;
                int asyncCount = 0;
                foreach (IDTSPath100 path in mainPipe.PathCollection)
                {
                    if (path.StartPoint.SynchronousInputID != 0)
                    {
                        continue;
                    }
                    var compInfo = PackageHelper.GetComponentInfo(path.StartPoint.Component);
                    if (compInfo != null && compInfo.ComponentType != DTSPipelineComponentType.SourceAdapter)
                    {
                        asyncCount++;
                    }
                }
                if (asyncCount > 0)
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, "There are {0} asynchronous outputs in the {1} data flow. Too many asynchronous outputs can adversely impact performance.", asyncCount, pipe.Name);
                    this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, -1));
                }
            }
        }
    }
}
