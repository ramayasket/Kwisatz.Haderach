﻿using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Encrypts and decrypts data using AES (Rijndael).
    /// </summary>
    public static class RijndaelCrypting
    {
        // This constant is used to determine the key size of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        const int KEYSIZE = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        const int DerivationIterations = 1000;

        public static byte[] Encrypt(byte[] input, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is prepended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(KEYSIZE / 8);

                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(input, 0, input.Length);
                                cryptoStream.FlushFinalBlock();

                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var output = saltStringBytes;

                                output = output.Concat(ivStringBytes).ToArray();
                                output = output.Concat(memoryStream.ToArray()).ToArray();

                                memoryStream.Close();
                                cryptoStream.Close();

                                return output;
                            }
                        }
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] input, string passPhrase)
        {
            // input is expected to be:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]

            // Get the salt bytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = input.Take(KEYSIZE / 8).ToArray();

            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = input.Skip(KEYSIZE / 8).Take(KEYSIZE / 8).ToArray();

            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = input.Skip((KEYSIZE / 8) * 2).Take(input.Length - ((KEYSIZE / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(KEYSIZE / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var output = new byte[cipherTextBytes.Length];

                                var decryptedByteCount = cryptoStream.Read(output, 0, output.Length);

                                memoryStream.Close();
                                cryptoStream.Close();

                                output = output.Take(decryptedByteCount).ToArray();

                                return output;
                            }
                        }
                    }
                }
            }
        }

        static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
