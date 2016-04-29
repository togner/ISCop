using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public enum SourceAccessMode : int
    {
        OpenRowSet = 0,
        OpenRowSetVariable = 1,
        SqlCommand = 2,
        SqlCommandVariable = 3
    }

    public class DataflowAccessMode : PackageRule
    {
        public DataflowAccessMode()
        {
            this.Id = "IS0103";
            this.Name = "DataflowAccessMode";
            this.Description = "Sources and Lookup transformations should not be set to use the 'Table or View' access mode, as it can be slower than specifying a SQL Statement.";
            this.ResultMessageFormat = "Component {0} uses OpenRowset access mode. {1}";
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
                foreach (IDTSComponentMetaData100 comp in mainPipe.ComponentMetaDataCollection)
                {
                    var compInfo = ComponentInfo.Create(comp);
                    if (compInfo != null
                        && (compInfo.Name == ComponentInfo.OleDbSourceName
                            || compInfo.Name == ComponentInfo.AdoNetSourceName
                            || compInfo.Name == ComponentInfo.LookupName))
                    {
                        SourceAccessMode? accesModeProp = null;
                        if (comp.TryGetCustomPropertyValue<SourceAccessMode?>(CustomPropertyNames.AccessMode, out accesModeProp))
                        {
                            if (accesModeProp.Value == SourceAccessMode.OpenRowSet
                                || accesModeProp.Value == SourceAccessMode.OpenRowSetVariable)
                            {
                                var msg = string.Format(CultureInfo.CurrentCulture, this.ResultMessageFormat, comp.Name, this.Description);
                                this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, comp.Name));
                            }
                        }
                    }
                }
            }
        }
    }
}
