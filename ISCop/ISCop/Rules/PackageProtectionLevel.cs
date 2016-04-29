using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class PackageProtectionLevel : PackageRule
    {
        public PackageProtectionLevel()
        {
            this.Id = "IS0004";
            this.Name = "PackageProtectionLevel";
            this.Description = "Consider using ServerStorage for packages stored in SQL Server, or DontSaveSensitive with appropriately secured configurations, as it makes packages easier to deploy and share with other developers.";
        }

        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            if (package.ProtectionLevel != DTSProtectionLevel.DontSaveSensitive
                && package.ProtectionLevel != DTSProtectionLevel.ServerStorage)
            {
                this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, this.Description, package.Name, null, -1));
            }
        }
    }
}