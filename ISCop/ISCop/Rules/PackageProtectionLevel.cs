using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop.Rules
{
    public class PackageProtectionLevel : PackageRule
    {
        public PackageProtectionLevel()
        {
            this.Id = "BIDS0005";
            this.Name = "PackageProtectionLevel";
            this.Description = "Validates that the ProtectionLevel property is set to DontSaveSensitive or ServerStorage";
        }

        public override void Check(Package package)
        {
            if (package.ProtectionLevel != DTSProtectionLevel.DontSaveSensitive
                && package.ProtectionLevel != DTSProtectionLevel.ServerStorage)
            {
                var msg = "Consider using ServerStorage for packages stored in SQL Server, or DontSaveSensitive with appropriately secured configurations, as it makes packages easier to deploy and share with other developers.";
                this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, null, -1));
            }
        }
    }
}

