using System.Collections.Generic;
using System.Globalization;
using BIDSHelper.SSIS;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataFlowAsynchronousPaths : PackageRule
    {
        public DataFlowAsynchronousPaths()
        {
            this.Id = "BIDS0001";
            this.Name = "DataFlowAsynchronousPaths";
            this.Description = "Checks for asynchronous paths in the data flow";
        }

        public override void Check(Package package)
        {
            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);
            foreach (var pipe in pipelines)
            {
                var mainPipe = (MainPipe)pipe.InnerObject;
                int asyncCount = 0;
                foreach (IDTSPath100 path in mainPipe.PathCollection)
                {
                    string key = PackageHelper.GetComponentKey(path.StartPoint.Component);
                    if (path.StartPoint.SynchronousInputID != 0)
                    {
                        continue;
                    }
                    if (PackageHelper.ComponentInfos[key].ComponentType != DTSPipelineComponentType.SourceAdapter)
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
