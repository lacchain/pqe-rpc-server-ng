using System;
using System.IO;
using System.Security.Cryptography;

namespace IBCQC_NetCore.Encryption
{
    public class AESEncrypt
    {
        public byte[] Encrypt(byte[] toEncrypt, byte[] sharedSecretBytes, byte[] saltBytes, int iterations)
        {
            //if (string.IsNullOrEmpty(key))
            //    throw new ArgumentNullException("key");

            // We should get the salt from our randomness not some function som  modified in the srng call

            // Derive a new Salt and IV from the Key
            using (var keyDerivationFunction = new Rfc2898DeriveBytes(sharedSecretBytes, saltBytes, iterations))
            {
                var keyBytes = keyDerivationFunction.GetBytes(32);
                var ivBytes = keyDerivationFunction.GetBytes(16);

                // Create an encryptor to perform the stream transform.
                // Create the streams used for encryption.
                using (var symmetricManaged = new AesCryptoServiceProvider())
                using (var encryptor = symmetricManaged.CreateEncryptor(keyBytes, ivBytes))
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        // Send the data through the CryptoStream, to the underlying MemoryStream
                        cryptoStream.Write(toEncrypt, 0, toEncrypt.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    int saltSize = saltBytes.Length;

                    // Return the encrypted bytes from the memory stream, in Base64 form so we can send it right to a database (if we want).
                    var cipherTextBytes = memoryStream.ToArray();

                    Array.Resize(ref saltBytes, saltSize + cipherTextBytes.Length);

                    // Copy the encrypted data into the saltbytes array leaving saltbytes as the leading byte array
                    Array.Copy(cipherTextBytes, 0, saltBytes, saltSize, cipherTextBytes.Length);

                    return saltBytes;
                }
            }
        }
    }
}
