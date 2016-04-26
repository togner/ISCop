using System;
using System.Linq;
using System.Security.Cryptography;

namespace Togner.Common.Crypto
{
    /// <summary>
    /// Wrapper for PBKDF2.
    /// Should be used instead of (fast/bad) SHA-1 or SHA-2.
    /// </summary>
    public static class SlowHash
    {
        /// <summary>
        /// Computes hash key for given text.
        /// </summary>
        /// <param name="plaintext">The text to generate hash for.</param>
        /// <param name="saltSize">The size of key in bytes. Must be at least 16 bytes.</param>
        /// <param name="saltSize">The size of salt in bytes. Must be at least 8 bytes.</param>
        /// <param name="key">The resulting hash.</param>
        /// <param name="salt">The random generated salt.</param>
        public static byte[] ComputeHash(string plaintext, int keySize, byte[] salt)
        {
            if (keySize < 16)
            {
                throw new ArgumentException("keySize must be at least 16");
            }
            using (var deriveBytes = new Rfc2898DeriveBytes(plaintext, salt))
            {
                return deriveBytes.GetBytes(keySize);
            }
        }

        /// <summary>
        /// Computes hash key for given text.
        /// </summary>
        /// <param name="plaintext">The text to generate hash for.</param>
        /// <param name="saltSize">The size of key in bytes. Must be at least 16 bytes.</param>
        /// <param name="saltSize">The size of salt in bytes. Must be at least 8 bytes.</param>
        /// <param name="key">The resulting hash.</param>
        /// <param name="salt">The random generated salt.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
        public static void ComputeHash(string plaintext, int keySize, int saltSize, out byte[] key, out byte[] salt)
        {
            if (keySize < 16)
            {
                throw new ArgumentException("keySize must be at least 16");
            }

            if (saltSize < 8)
            {
                throw new ArgumentException("saltSize must be at least 8");
            }

            // specify that we want to randomly generate a x-byte salt
            using (var deriveBytes = new Rfc2898DeriveBytes(plaintext, saltSize))
            {
                salt = deriveBytes.Salt;
                key = deriveBytes.GetBytes(keySize);
            }
        }

        /// <summary>
        /// Verifies whether the key/salt combination matches the text.
        /// </summary>
        /// <param name="key">The key to match.</param>
        /// <param name="salt">The salt to match.</param>
        /// <param name="plaintext">The text to match against.</param>
        /// <returns>True if the key/salt are valid for given text, false otherwise.</returns>
        public static bool Verify(byte[] key, byte[] salt, string plaintext)
        {
            if (key == null)
            {
                return false;
            }
            var textKey = SlowHash.ComputeHash(plaintext, key.Length, salt);
            return textKey.SequenceEqual(key);
        }
    }
}
