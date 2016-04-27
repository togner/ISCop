using System;
using System.Collections.Generic;
using System.Globalization;
using BIDSHelper.SSIS;
using log4net;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowAccessMode : PackageRule
    {
        public DataflowAccessMode()
        {
            this.Id = "BIDS0004";
            this.Name = "AccessMode";
            this.Description = "Validates that sources and Lookup transformations are not set to use the 'Table or View' access mode, as it can be slower than specifying a SQL Statement";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OpenRowset"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            foreach (var pipe in PackageHelper.GetControlFlowObjects<MainPipe>(package))
            {
                var mainPipe = (MainPipe)pipe.InnerObject;
                foreach (IDTSComponentMetaData100 comp in mainPipe.ComponentMetaDataCollection)
                {
                    var compInfo = PackageHelper.GetComponentInfo(comp);
                    if (compInfo != null 
                        && (compInfo.Name == PackageRule.OleDbSourceComponentName 
                            || compInfo.Name == PackageRule.AdoNetSourceComponentName 
                            || compInfo.Name == PackageRule.LookupComponentName))
                    {
                        IDTSCustomProperty100 prop = null;
                        try
                        {
                            // Not all Lookup comps have this property
                            prop = comp.CustomPropertyCollection["AccessMode"]; 
                        }
                        catch 
                        {
                        }
                        if (prop != null && prop.Value is int)
                        {
                            var accesModeProp = (SourceAccessMode)prop.Value;
                            if (accesModeProp == SourceAccessMode.OpenRowSet
                                || accesModeProp == SourceAccessMode.OpenRowSetVariable)
                            {
                                var msg = string.Format(CultureInfo.CurrentCulture, "Change the {0} component to use a SQL Command access mode, as this performs better than the OpenRowset access mode.", comp.Name);
                                this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, comp.Name));
                            }
                        }
                    }
                }
            }
        }
    }
}
