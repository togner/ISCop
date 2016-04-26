using System.Security;

namespace Togner.Common.Crypto
{
    /// <summary>
    /// Extensions for Crypto namespace.
    /// </summary>
    public static class CryptoExtensions
    {
        /// <summary>
        /// Converts object to SecureString.
        /// </summary>
        /// <param name="value">Object to convert.</param>
        /// <returns>New SecureString on success, null the object is null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static SecureString ToSecureString(this object value)
        {
            if (value == null)
            {
                return null;
            }
            var valueString = value.ToString();
            var result = new SecureString();
            foreach (var c in valueString)
            {
                result.AppendChar(c);
            }
            result.MakeReadOnly();
            return result;
        }
    }
}
