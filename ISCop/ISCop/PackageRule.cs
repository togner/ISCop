using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop
{
    public abstract class PackageRule
    {
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
