using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowFastLoadCheckConstraints : PackageRule
    {
        public DataflowFastLoadCheckConstraints()
        {
            this.Id = "IS0108";
            this.Name = "DataflowFastLoadCheckConstraints";
            this.Description = "If the destination uses Fast Load, it must have Check constraints option set. Otherwise the constraints will become NOCHECK.";
            this.ResultMessageFormat = "Destination component \"{0}\" doesn't set CHECK_CONSTRAINTS. {1}";
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
                        && (compInfo.Name == ComponentInfo.OleDbDestinationName))
                    {
                        DestinationAccessMode? accessMode = null;
                        string fastLoadOptions = null;
                        if (comp.TryGetCustomPropertyValue<DestinationAccessMode?>(CustomPropertyNames.AccessMode, out accessMode)
                            && comp.TryGetCustomPropertyValue<string>(CustomPropertyNames.FastLoadOptions, out fastLoadOptions)
                            && (accessMode.Value == DestinationAccessMode.OpenRowSetFastLoad
                                || accessMode.Value == DestinationAccessMode.OpenRowSetFastLoadVariable)
                            && !fastLoadOptions.ToUpperInvariant().Contains("CHECK_CONSTRAINTS"))
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