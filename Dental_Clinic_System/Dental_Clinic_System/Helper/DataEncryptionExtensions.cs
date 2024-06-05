using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Dental_Clinic_System.Helper
{
	public static class DataEncryptionExtensions
	{
		#region [Hashing Extension]
		public static string ToSHA256Hash(this string password, string? saltKey)
		{
			var sha256 = SHA256.Create();
			byte[] encryptedSHA256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
			sha256.Clear();

			return Convert.ToBase64String(encryptedSHA256).Substring(0, 30);
		}

        public static string ToSHA256Hash(this string password)
        {
            string? saltKey = "DONOTTOUCHOURPASSWORD!!!";
            var sha256 = SHA256.Create();
            byte[] encryptedSHA256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
            sha256.Clear();

            return Convert.ToBase64String(encryptedSHA256).Substring(0, 20);
        }

        public static string ToSHA512Hash(this string password, string? saltKey)
		{
			SHA512Managed sha512 = new SHA512Managed();
			byte[] encryptedSHA512 = sha512.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
			sha512.Clear();

			return Convert.ToBase64String(encryptedSHA512).Substring(0, 30);
		}

		public static string ToMd5Hash(this string password, string? saltKey)
		{
			using (var md5 = MD5.Create())
			{
				byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
				StringBuilder sBuilder = new StringBuilder();
				for (int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}

				return sBuilder.ToString().Substring(0, 30);
			}
		}

        public static string ToMd5Hash(this string password)
        {
            using (var md5 = MD5.Create())
            {
				string? saltKey = "KATH&DATH2&LUCIA";
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString().Substring(0, 30);
            }
        }

        #endregion


        public static string Encrypt(string plainText, in string key = "DONOTTOUCHOURPASSWORD!!!", in string iv = "1234567890123456")
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, in string key = "DONOTTOUCHOURPASSWORD!!!", in string iv = "1234567890123456")
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

}
