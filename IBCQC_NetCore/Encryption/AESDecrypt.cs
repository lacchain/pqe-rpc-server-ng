using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace IBCQC_NetCore.Encryption
{
    public class AESDecrypt
    {
        public byte[] AESDecryptBytes(byte[] toDecrypt, string sharedSecretString, int saltSize, int iterations)
        {
            if (string.IsNullOrEmpty(sharedSecretString))
                throw new ArgumentNullException("sharedSecretString");

            // Extract the salt from our ciphertext
            var saltBytes = toDecrypt.Take(saltSize).ToArray();
            var ciphertextBytes = toDecrypt.Skip(saltSize).Take(toDecrypt.Length - saltSize).ToArray();

            using (var keyDerivationFunction = new Rfc2898DeriveBytes(sharedSecretString, saltBytes, iterations))
            {
                // Derive the previous IV from the Key and Salt
                var keyBytes = keyDerivationFunction.GetBytes(32);
                var ivBytes = keyDerivationFunction.GetBytes(16);

                // Create a decrytor to perform the stream transform.
                // Create the streams used for decryption.
                // The default Cipher Mode is CBC and the Padding is PKCS7 which are both good
               

                // Changed to use no padding
                using (var symmetricManaged = new AesManaged() { Padding = PaddingMode.None })

                using (var decryptor = symmetricManaged.CreateDecryptor(keyBytes, ivBytes))

                using (var memoryStream = new MemoryStream(ciphertextBytes))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    var results = new byte[ciphertextBytes.Length];

                    // Return the decrypted bytes from the decrypting stream.
                    cryptoStream.Read(results, 0, ciphertextBytes.Length);
                    cryptoStream.Flush();

                    return results;
                }
            }
        }


    }
}
