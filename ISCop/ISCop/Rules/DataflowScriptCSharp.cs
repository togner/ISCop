using System.Globalization;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class DataflowScriptCSharp : ScriptTaskCSharp
    {
        public DataflowScriptCSharp()
        {
            this.Id = "SSIS0001";
            this.Name = "DataflowScriptCSharp";
            this.Description = "Every data flow script component must be written in C#.";
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
                    if (compInfo != null && compInfo.Name == ComponentInfo.ScriptName)
                    {
                        var language = comp.GetCustomPropertyValue<string>(CustomPropertyNames.ScriptLanguage);
                        if (language != ScriptTaskCSharp.CSharp)
                        {
                            var msg = string.Format(CultureInfo.CurrentCulture, "Script component {0} is written in {1}.", comp.Name, language);
                            this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, pipe.Name, comp.Name));
                        }
                    }
                }
            }
        }
    }
}
