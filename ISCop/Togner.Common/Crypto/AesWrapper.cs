using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Togner.Common.Crypto
{
    /// <summary>
    /// AES implementation from http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net/2791259#2791259
    /// Uses base64 to convert between bytes and strings.
    /// </summary>
    public class AesWrapper
    {
        private readonly byte[] _salt;

        /// <summary>
        /// Initializes a new instance of the AesWrapper class.
        /// </summary>
        /// <param name="salt">The salt base64 string - should be generated randomly and stored along the encrypted data.</param>
        public AesWrapper(string salt)
        {
            if (string.IsNullOrWhiteSpace(salt))
            {
                throw new ArgumentNullException("salt");
            }
            this._salt = Encoding.UTF8.GetBytes(salt);
        }

        /// <summary>
        /// Encrypts given string using AES.  
        /// The string can be decrypted using DecryptStringAES().  
        /// The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plaintext">The text to encrypt (UTF8).</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        /// <returns>Encrypted string (base64).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string EncryptString(string plaintext, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plaintext))
            {
                throw new ArgumentNullException("plaintext");
            }
            if (string.IsNullOrEmpty(sharedSecret))
            {
                throw new ArgumentNullException("sharedSecret");
            }

            string result = null;
            using (var aesAlg = new RijndaelManaged())
            {
                // generate the key from the shared secret and the salt
                aesAlg.Key = SlowHash.ComputeHash(sharedSecret, aesAlg.KeySize / 8, this._salt);

                // Create a decrytor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var memoryStream = new MemoryStream())
                {
                    // prepend the IV
                    memoryStream.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    memoryStream.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var writerStream = new StreamWriter(cryptoStream, Encoding.UTF8))
                        {
                            // Write all data to the stream.
                            writerStream.Write(plaintext);
                        }
                    }
                    result = Convert.ToBase64String(memoryStream.ToArray());
                }
            }

            // Return the encrypted bytes from the memory stream.
            return result;
        }

        /// <summary>
        /// Decrypts given string.  
        /// Assumes the string was encrypted using EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt (base64).</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        /// <returns>Decrypted string (UTF8).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string DecryptString(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentNullException("cipherText");
            }
            if (string.IsNullOrEmpty(sharedSecret))
            {
                throw new ArgumentNullException("sharedSecret");
            }

            string result = null;
            using (var aesAlg = new RijndaelManaged())
            {
                aesAlg.Key = SlowHash.ComputeHash(sharedSecret, aesAlg.KeySize / 8, this._salt);

                // Create the streams used for decryption.                
                var bytes = Convert.FromBase64String(cipherText);
                using (var memoryStream = new MemoryStream(bytes))
                {
                    aesAlg.IV = ReadByteArray(memoryStream);

                    // Create a decrytor to perform the stream transform.
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var readerStream = new StreamReader(cryptoStream, Encoding.UTF8))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            result = readerStream.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Decrypts given string.  
        /// Assumes the string was encrypted using EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt (base64).</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        /// <returns>Decrypted secure string.</returns>
        public SecureString DecryptSecureString(string cipherText, string sharedSecret)
        {
            var unsecureResult = this.DecryptString(cipherText, sharedSecret);
            if (unsecureResult != null)
            {
                return unsecureResult.ToSecureString();
            }
            else
            {
                return null;
            }
        }

        private static byte[] ReadByteArray(Stream s)
        {
            var rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new InvalidOperationException("Stream did not contain properly formatted byte array");
            }
            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new InvalidOperationException("Did not read byte array properly");
            }
            return buffer;
        }
    }
}
