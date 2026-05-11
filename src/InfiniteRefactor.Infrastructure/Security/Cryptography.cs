using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace InfiniteRefactor.Infrastructure.Security
{
    public static class Cryptography
    {
        public static string UPPER_CASE_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string LOWER_CASE_LETTERS = "abcdefghijklmnopqrstuvwxyz";
        public static string NUMBERS = "0123456789";
        public static string CreateRandomString(int length, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            var code = string.Empty;
            var rnd = new Random();
            for (var i = 0; i < length; i++)
            {
                code += chars[rnd.Next(chars.Length - 1)];
            }
            return code;
        }

        public static string NumberToString(int value, int? limitLenth = null, string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            if (limitLenth.HasValue)
            {
                if (value >= Math.Pow(chars.Length, limitLenth.Value)) { throw new ArgumentException("值不在允许范围"); }
            }
            var i = value / chars.Length;
            var mod = value % chars.Length;

            if (i == 0)
            {
                return chars[mod].ToString();
            }
            else if (i < chars.Length)
            {
                return chars[i].ToString() + chars[mod].ToString();
            }
            else
            {
                return NumberToString(i, limitLenth, chars) + chars[mod].ToString();
            }
        }

        public static int StringToNumber(string value, string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            int result = 0;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                var v = chars.IndexOf(value[value.Length - i - 1]);
                if (v < 0) { throw new ArgumentException("值不在允许范围"); }

                result += v * Convert.ToInt32(Math.Pow(chars.Length, i));
            }
            return result;
        }

        #region Hash
        public static HashAlgorithm CreateHashAlgorithmFromName(string name) => CryptoConfig.CreateFromName(name) as HashAlgorithm;

        public static string Hash(HashAlgorithm hashAlgorithm, byte[] rawText)
        {
            rawText = hashAlgorithm.ComputeHash(rawText);
            return BitConverter.ToString(rawText).Replace("-", "").ToLower();
        }

        public static string Hash(HashAlgorithm hashAlgorithm, string text, string salt = "", Encoding encoding = null)
        {
            var enc = encoding ?? Encoding.UTF8;
            if (!string.IsNullOrWhiteSpace(salt))
            {
                var sb = new StringBuilder();
                for (var i = 0; i < Math.Max(text.Length, salt.Length); i++)
                {
                    if (text.Length > i) { sb.Append(text[i]); }
                    else { sb.Append("0"); }
                    if (salt.Length > i) { sb.Append(salt[i]); }
                    else { sb.Append("0"); }
                }
                text = sb.ToString();
            }

            byte[] bs = enc.GetBytes(text);
            bs = hashAlgorithm.ComputeHash(bs);
            //hashAlgorithm.Clear();

            return BitConverter.ToString(bs).Replace("-", "").ToLower();
        }

        public static string Hash(string hashName, string text, string salt = "", Encoding encoding = null)
        {
            using (var hash = CreateHashAlgorithmFromName(hashName))
            {
                return Hash(hash, text, salt, encoding);
            }
        }

        public static string KeyedHash(string hashName, string key, string text, string salt = "", Encoding encoding = null)
        {
            using (var hash = CreateHashAlgorithmFromName(hashName))
            {
                var keyedHashAlg = hash as KeyedHashAlgorithm;
                if (keyedHashAlg == null)
                {
                    throw new ArgumentException($"{hashName} is not a keyed hash algorithm.");
                }

                keyedHashAlg.Key = Encoding.UTF8.GetBytes(key);

                return Hash(keyedHashAlg, text, salt, encoding);
            }
        }

        public static string KeyedHash(string hashName, string key, byte[] rawText)
        {
            using (var hash = CreateHashAlgorithmFromName(hashName))
            {
                var keyedHashAlg = hash as KeyedHashAlgorithm;
                if (keyedHashAlg == null)
                {
                    throw new ArgumentException($"{hashName} is not a keyed hash algorithm.");
                }

                keyedHashAlg.Key = Encoding.UTF8.GetBytes(key);

                return Hash(keyedHashAlg, rawText);
            }
        }

        public static string MD5(this string text, string salt = "", Encoding encoding = null)
        {
            return Hash("MD5", text, salt, encoding);
        }

        public static string SHA1(this string text, string salt = "", Encoding encoding = null)
        {
            return Hash("SHA1", text, salt, encoding);
        }

        public static string HMACSHA256(string key, string text, string salt = "", Encoding encoding = null)
        {
            return KeyedHash("HMACSHA256", key, text, salt, encoding);
        }

        // public static string Sm3Hash(this string data, Encoding encoding = null)
        // {
        //     encoding = encoding ?? Encoding.UTF8;
        //     var sm3Output = new byte[32];
        //     var sm3 = new SM3Digest();
        //     sm3.BlockUpdate(encoding.GetBytes(data));
        //     sm3.DoFinal(sm3Output, 0);
        //     return encoding.GetString(Hex.Encode(sm3Output));
        // }
        #endregion

        public static SymmetricAlgorithm CreateSymmetricAlgorithmFromName(string name) => CryptoConfig.CreateFromName(name) as SymmetricAlgorithm;

        public static byte[] Encrypt(string algName, byte[] key, byte[] plainText, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            var alg = CreateSymmetricAlgorithmFromName(algName);
            alg.Mode = mode;
            alg.Padding = padding;

            var match = alg.LegalKeySizes.Where(t => t.MaxSize >= key.Length * 8).OrderBy(t => (key.Length * 8 - t.MinSize) + (t.MaxSize - key.Length * 8)).FirstOrDefault();
            if (match == null) { throw new ArgumentOutOfRangeException("key长度不符"); }
            for (int i = match.MinSize; i <= match.MaxSize; i += match.SkipSize)
            {
                if (i >= key.Length * 8)
                {
                    var keyBytes = new byte[i / 8];
                    Array.Copy(key, keyBytes, Math.Min(key.Length, keyBytes.Length));

                    alg.Key = keyBytes;
                    alg.IV = new byte[alg.IV.Length];
                    if (iv != null)
                    {
                        Array.Copy(iv, alg.IV, Math.Min(iv.Length, alg.IV.Length));
                    }

                    ICryptoTransform transform = alg.CreateEncryptor();
                    return transform.TransformFinalBlock(plainText, 0, plainText.Length);
                }
            }
            throw new Exception();
        }

        public static byte[] Decrypt(string algName, byte[] key, byte[] plainText, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            var alg = CreateSymmetricAlgorithmFromName(algName);
            alg.Mode = mode;
            alg.Padding = padding;

            var match = alg.LegalKeySizes.Where(t => t.MaxSize >= key.Length * 8).OrderBy(t => (key.Length * 8 - t.MinSize) + (t.MaxSize - key.Length * 8)).FirstOrDefault();
            if (match == null) { throw new ArgumentOutOfRangeException("key长度不符"); }
            for (int i = match.MinSize; i <= match.MaxSize; i += match.SkipSize)
            {
                if (i >= key.Length * 8)
                {
                    var keyBytes = new byte[i / 8];
                    Array.Copy(key, keyBytes, Math.Min(key.Length, keyBytes.Length));

                    alg.Key = keyBytes;
                    alg.IV = new byte[alg.IV.Length];
                    if (iv != null)
                    {
                        Array.Copy(iv, alg.IV, Math.Min(iv.Length, alg.IV.Length));
                    }

                    ICryptoTransform transform = alg.CreateDecryptor();
                    return transform.TransformFinalBlock(plainText, 0, plainText.Length);
                }
            }
            throw new Exception();
        }

        public static string Encrypt(string algName, string key, string text, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            return Convert.ToBase64String(Encrypt(algName, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(text), iv, mode, padding));
        }

        public static string Decrypt(string algName, string key, string text, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            return Encoding.UTF8.GetString(Decrypt(algName, Encoding.UTF8.GetBytes(key), Convert.FromBase64String(text), iv, mode, padding));
        }

        public static CryptoStream CreateSymmetricCryptoStream(string algName, string password, Stream baseStream, CryptoStreamMode mode)
        {
            var alg = CreateSymmetricAlgorithmFromName(algName);
            var key = Encoding.ASCII.GetBytes(password.PadRight(32));
            var vi = Encoding.ASCII.GetBytes("AIRMASTER".PadRight(16));
            alg.Key = key;
            return new CryptoStream(baseStream, mode == CryptoStreamMode.Write ? alg.CreateEncryptor(key, vi) : alg.CreateDecryptor(key, vi), mode);
        }

        [Obsolete("TripleDES is insecure. Use AES-based Encrypt/Decrypt methods instead.")]
        [SupportedOSPlatform("windows")]
        public static string TripleDESEncrypt(string publicKey, string strSource)
        {
            throw new NotSupportedException("TripleDES is disabled due to security risks. Use the AES-based Encrypt/Decrypt methods instead.");
        }

        [Obsolete("TripleDES is insecure. Use AES-based Encrypt/Decrypt methods instead.")]
        [SupportedOSPlatform("windows")]
        public static string TripleDESDecrypt(string publicKey, string data)
        {
            throw new NotSupportedException("TripleDES is disabled due to security risks. Use the AES-based Encrypt/Decrypt methods instead.");
        }

        #region RijndaelManaged
        /// <summary>
        /// RijndaelManaged
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <param name="iv"></param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        [Obsolete]
        public static string AesEncrypt(string key, string text, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text)) { return null; }
            return AesEncrypt((encoding ?? Encoding.UTF8).GetBytes(key), (encoding ?? Encoding.UTF8).GetBytes(text), iv, mode, padding);
        }
        /// <summary>
        /// RijndaelManaged
        /// </summary>
        /// <param name="keyArray"></param>
        /// <param name="textArray"></param>
        /// <param name="iv"></param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        [Obsolete]
        public static string AesEncrypt(byte[] keyArray, byte[] textArray, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
        {
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = keyArray,
                Mode = mode,
                Padding = padding,
                IV = iv
            };

            var cTransform = rm.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(textArray, 0, textArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// RijndaelManaged
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <param name="iv"></param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        [Obsolete]
        public static string AesDecrypt(string key, string text, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(text)) { return null; }
            return AesDecrypt((encoding ?? Encoding.UTF8).GetBytes(key), Convert.FromBase64String(text), iv, mode, padding);
        }
        /// <summary>
        /// RijndaelManaged
        /// </summary>
        /// <param name="keyArray"></param>
        /// <param name="base64Array"></param>
        /// <param name="iv"></param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        [Obsolete]
        public static string AesDecrypt(byte[] keyArray, byte[] base64Array, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, Encoding encoding = null)
        {
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = keyArray,
                Mode = mode,
                Padding = padding,
                IV = iv
            };

            var cTransform = rm.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(base64Array, 0, base64Array.Length);

            return (encoding ?? Encoding.UTF8).GetString(resultArray);
        }
        #endregion

        #region RSA (OAEP-SHA256)

        private static System.Security.Cryptography.RSA LoadRSA(string pemOrXml)
        {
            var rsa = System.Security.Cryptography.RSA.Create();
            if (pemOrXml.TrimStart().StartsWith("<"))
                rsa.FromXmlString(pemOrXml);
            else
                rsa.ImportFromPem(pemOrXml);
            return rsa;
        }

        public static byte[] RSAEncrypt(string pemOrXml, byte[] data)
        {
            using var rsa = LoadRSA(pemOrXml);
            int blockSize = rsa.KeySize / 8 - 42; // OAEP-SHA256 overhead
            if (data.Length <= blockSize)
                return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);

            using var output = new MemoryStream();
            int offset = 0;
            while (offset < data.Length)
            {
                int len = Math.Min(blockSize, data.Length - offset);
                var block = rsa.Encrypt(data[offset..(offset + len)], RSAEncryptionPadding.OaepSHA256);
                output.Write(block, 0, block.Length);
                offset += len;
            }
            return output.ToArray();
        }

        public static string RSAEncrypt(string pemOrXml, string text)
            => Convert.ToBase64String(RSAEncrypt(pemOrXml, Encoding.UTF8.GetBytes(text)));

        public static byte[] RSADecrypt(string pemOrXml, byte[] data)
        {
            using var rsa = LoadRSA(pemOrXml);
            int blockSize = rsa.KeySize / 8;
            if (data.Length <= blockSize)
                return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);

            using var output = new MemoryStream();
            int offset = 0;
            while (offset < data.Length)
            {
                int len = Math.Min(blockSize, data.Length - offset);
                var block = rsa.Decrypt(data[offset..(offset + len)], RSAEncryptionPadding.OaepSHA256);
                output.Write(block, 0, block.Length);
                offset += len;
            }
            return output.ToArray();
        }

        public static string RSADecrypt(string pemOrXml, string base64)
            => Encoding.UTF8.GetString(RSADecrypt(pemOrXml, Convert.FromBase64String(base64)));

        public static byte[] RSASign(string pemOrXml, string hash, byte[] data)
        {
            using var rsa = LoadRSA(pemOrXml);
            return rsa.SignData(data, new HashAlgorithmName(hash.ToUpperInvariant()), RSASignaturePadding.Pkcs1);
        }

        public static bool RSAVerify(string pemOrXml, string hash, byte[] data, byte[] signature)
        {
            using var rsa = LoadRSA(pemOrXml);
            return rsa.VerifyData(data, signature, new HashAlgorithmName(hash.ToUpperInvariant()), RSASignaturePadding.Pkcs1);
        }

        #endregion
    }
}
