using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop
{
    public abstract class PackageRule
    {
        protected const string OleDbSourceComponentName = "OLE DB Source";
        protected const string AdoNetSourceComponentName = "ADO NET Source";
        protected const string SortComponentName = "DTSTransform.Sort.2";
        protected const string LookupComponentName = "Lookup";

        public IList<Result> Results { get; private set; }
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        protected PackageRule()
        {
            this.Results = new List<Result>();
        }

        public abstract void Check(Package package);
    }
}
