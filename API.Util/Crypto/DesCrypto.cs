using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Util.Crypto
{
    public class DesCrypto
    {
        private static byte[] _key = Encoding.ASCII.GetBytes("Diademata");
        public static string Encryption(string plainText)
        {
            DESCryptoServiceProvider _crypto = new DESCryptoServiceProvider();
            _crypto.IV = _key;
            _crypto.Key = _key;
            string encryptedText = "";
            try
            {
                using (var ms = new MemoryStream())
                {
                    var cryStream = new CryptoStream(ms, _crypto.CreateEncryptor(), CryptoStreamMode.Write);
                    byte[] bytes = Encoding.Unicode.GetBytes(plainText.ToCharArray());
                    cryStream.Write(bytes, 0, bytes.Length);
                    cryStream.FlushFinalBlock();
                    encryptedText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return encryptedText;
        }
        public static string Decryption(string encryptedText)
        {
            DESCryptoServiceProvider _crypto = new DESCryptoServiceProvider();
            _crypto.IV = _key;
            _crypto.Key = _key;
            string decryptedText = "";
            try
            {
                using (var ms = new MemoryStream())
                {
                    var cryStream = new CryptoStream(ms, _crypto.CreateDecryptor(), CryptoStreamMode.Write);
                    byte[] bytes = Encoding.Unicode.GetBytes(decryptedText);
                    cryStream.Write(bytes, 0, bytes.Length);
                    cryStream.FlushFinalBlock();
                    decryptedText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return decryptedText;
        }
    }
}
