using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GodotModules
{
    public static class EncryptionHelper
    {
        public static string Encrypt(string clearText)
        {
            var EncryptionKey = "secret";

            if (string.IsNullOrEmpty(clearText))
                return clearText;

            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using var encryptor = Aes.Create();
        
            var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
            
            cs.Write(clearBytes, 0, clearBytes.Length);
            
            clearText = Convert.ToBase64String(ms.ToArray());
            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            var EncryptionKey = "secret";

            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            cipherText = cipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);
            var encryptor = Aes.Create();
            
            var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write);
            
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.Close();

            cipherText = Encoding.Unicode.GetString(ms.ToArray());
            return cipherText;
        }
    }
}