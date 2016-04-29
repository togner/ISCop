using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowAsynchronousPaths : PackageRule
    {
        
        public DataflowAsynchronousPaths()
        {
            this.Id = "IS104";
            this.Name = "DataflowAsynchronousPaths";
            this.Description = "Too many asynchronous outputs can adversely impact performance.";
            this.ResultMessageFormat = "There are {0} asynchronous outputs in the {1} data flow. {2}";
        }

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var pipe in package.GetControlFlowObjects<MainPipe>())
            {
                var mainPipe = (MainPipe)pipe.InnerObject;
                int asyncCount = 0;
                foreach (IDTSPath100 path in mainPipe.PathCollection)
                {
                    if (path.StartPoint.SynchronousInputID != 0)
                    {
                        continue;
                    }
                    var compInfo = ComponentInfo.Create(path.StartPoint.Component);
                    if (compInfo != null && compInfo.ComponentType != DTSPipelineComponentType.SourceAdapter)
                    {
                        asyncCount++;
                    }
                }
                if (asyncCount > 0)
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, this.ResultMessageFormat, asyncCount, pipe.Name, this.Description);
                    this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, -1));
                }
            }
        }
    }
}
