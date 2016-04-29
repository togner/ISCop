using System.Collections.Generic;
using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowSortTransformations : PackageRule
    {
        public DataflowSortTransformations()
        {
            this.Id = "BIDS0003";
            this.Name = "DataFlowSortTransformations";
            this.Description = "Checks for the number of sorts in the data flows, and whether they could be performed in a database";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SortKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IsSorted"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)")]
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
                                var msg = string.Format(CultureInfo.CurrentCulture, "The {0} Sort transformation is operating on data provided from the {1} source. Rather than using the Sort transformation, which is fully blocking, the sorting should be performed using a WHERE clause in the source's SQL, and the IsSorted and SortKey properties should be set appropriately. Reference: http://msdn.microsoft.com/en-us/library/ms137653(SQL.90).aspx", comp.Name, sourceComp.Name);
                                this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, comp.Name));
                            }
                        }
                    }
                }
                if (sortCount > 2)
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, "There are {0} Sort transformations in the {1} data flow. A large number of Sorts can slow down data flow performance. Consider staging the data to a relational database and sorting it there.", sortCount, pipe.Name);
                    this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, -1));
                }
            }
        }
    }
}
