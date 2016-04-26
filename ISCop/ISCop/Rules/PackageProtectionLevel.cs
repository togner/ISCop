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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ServerStorage"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Result.#ctor(ISCop.ResultType,System.String,System.String,System.String,System.String,System.String,System.Int32)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DontSaveSensitive")]
        public override void Check(Package package)
        {
            if (package == null)
            {
                return;
            }
            if (package.ProtectionLevel != DTSProtectionLevel.DontSaveSensitive
                && package.ProtectionLevel != DTSProtectionLevel.ServerStorage)
            {
                var msg = "Consider using ServerStorage for packages stored in SQL Server, or DontSaveSensitive with appropriately secured configurations, as it makes packages easier to deploy and share with other developers.";
                this.Results.Add(new Result(ResultType.Warning, this.Id, this.Name, msg, package.Name, null, -1));
            }
        }
    }
}

