using System.Collections.Generic;
using System.Globalization;
using BIDSHelper.SSIS;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataFlowSortTransformations : PackageRule
    {
        public DataFlowSortTransformations()
        {
            this.Id = "BIDS0003";
            this.Name = "DataFlowSortTransformations";
            this.Description = "Checks for the number of sorts in the data flows, and whether they could be performed in a database";
        }

        public override void Check(Package package)
        {
            List<TaskHost> pipelines = PackageHelper.GetControlFlowObjects<MainPipe>(package);
            foreach (var pipe in pipelines)
            {
                var mainPipe = (MainPipe)pipe.InnerObject;
                int sortCount = 0;
                foreach (IDTSComponentMetaData100 comp in mainPipe.ComponentMetaDataCollection)
                {
                    string key = PackageHelper.GetComponentKey(comp);
                    if (PackageHelper.ComponentInfos[key].CreationName == PackageRule.SortComponentName)
                    {
                        sortCount++;

                        //Trace the input
                        IDTSComponentMetaData100 sourceComp = PackageHelper.TraceInputToSource(mainPipe, comp);
                        if (sourceComp != null)
                        {
                            key = PackageHelper.GetComponentKey(sourceComp);
                            if (PackageHelper.ComponentInfos[key].Name == PackageRule.OleDbSourceComponentName
                                || PackageHelper.ComponentInfos[key].Name == PackageRule.AdoNetSourceComponentName)
                            {
                                var msg = string.Format(CultureInfo.CurrentCulture, "The {0} Sort transformation is operating on data provided from the {1} source. Rather than using the Sort transformation, which is fully blocking, the sorting should be performed using a WHERE clause in the source's SQL, and the IsSorted and SortKey properties should be set appropriately. Reference: http://msdn.microsoft.com/en-us/library/ms137653(SQL.90).aspx", sortCount, sourceComp.Name);
                                this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, -1));
                            }
                        }
                    }
                }
                if (sortCount > 2)
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, "There are {0} Sort transfomations in the {1} data flow. A large number of Sorts can slow down data flow performance. Consider staging the data to a relational database and sorting it there.", sortCount, pipe.Name);
                    this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, -1));
                }
            }
        }
    }
}
