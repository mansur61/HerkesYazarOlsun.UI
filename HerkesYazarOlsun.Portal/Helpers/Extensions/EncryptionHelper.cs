namespace HerkesYazarOlsun.Portal.Helpers.Extensions
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class EncryptionHelper
    {
        private static readonly string key = "herkses-yazar-olsun"; // 32 byte key (256 bit)
        // private static readonly string iv = "your-16-byte-iv"; 
        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.GenerateIV(); // Rastgele IV oluştur

                var iv = aesAlg.IV;
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(iv, 0, iv.Length); // IV'yi şifreli verinin başına yaz

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    var encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                var iv = new byte[aesAlg.BlockSize / 8]; // IV uzunluğunu belirle
                Array.Copy(fullCipher, 0, iv, 0, iv.Length); // IV'yi şifreli veriden ayıkla

                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = iv;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

}
