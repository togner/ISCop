using System;
using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowSortTransformations : PackageRule
    {
        private static readonly Tuple<string, string, string, string> DataflowSortTransformationRule = new Tuple<string, string, string, string>("IS0106",
            "DataflowSortTransformation",
            "Sort transformation is fully blocking - sorting should be performed using a WHERE clause in the source's SQL, and the IsSorted and SortKey properties should be set appropriately. Reference: http://msdn.microsoft.com/en-us/library/ms137653.aspx",
            "The {0} Sort transformation is operating on data provided from the {1} source. {2}");
        private static readonly Tuple<string, string, string, string> DataflowSortTransformationCountRule = new Tuple<string, string, string, string>("IS0107",
            "DataflowSortTransformationCount",
            "A large number of Sorts can slow down data flow performance. Consider staging the data to a relational database and sorting it there.",
            "There are {0} Sort transformations in the {1} data flow. {2}");

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var pipe in package.GetControlFlowObjects<MainPipe>())
            {
                var mainPipe = (MainPipe)pipe.InnerObject;
                int sortCount = 0;
                foreach (IDTSComponentMetaData100 comp in mainPipe.ComponentMetaDataCollection)
                {
                    var compInfo = ComponentInfo.Create(comp);
                    if (compInfo != null && compInfo.CreationName == ComponentInfo.SortTransformName)
                    {
                        sortCount++;

                        //Trace the input
                        var sourceComp = comp.TraceInputToSource(mainPipe);
                        if (sourceComp != null)
                        {
                            var sourceCompInfo = ComponentInfo.Create(sourceComp);
                            if (sourceCompInfo != null
                                && (sourceCompInfo.Name == ComponentInfo.OleDbSourceName
                                    || sourceCompInfo.Name == ComponentInfo.AdoNetSourceName))
                            {
                                this.Results.Add(new Result(ResultType.Warning, 
                                    DataflowSortTransformations.DataflowSortTransformationRule.Item1, 
                                    DataflowSortTransformations.DataflowSortTransformationRule.Item2,
                                    string.Format(CultureInfo.CurrentCulture, DataflowSortTransformations.DataflowSortTransformationRule.Item4, comp.Name, sourceComp.Name, DataflowSortTransformations.DataflowSortTransformationRule.Item3),
                                    package.Name,
                                    pipe.Name, 
                                    comp.Name));
                            }
                        }
                    }
                }
                if (sortCount > 2)
                {
                    this.Results.Add(new Result(ResultType.Warning, 
                        DataflowSortTransformations.DataflowSortTransformationCountRule.Item1, 
                        DataflowSortTransformations.DataflowSortTransformationCountRule.Item2,
                        string.Format(CultureInfo.CurrentCulture, DataflowSortTransformations.DataflowSortTransformationCountRule.Item4, sortCount, pipe.Name, DataflowSortTransformations.DataflowSortTransformationCountRule.Item3),
                        package.Name, 
                        pipe.Name, 
                        -1));
                }
            }
        }
    }
}
