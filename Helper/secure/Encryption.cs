using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Helper.secure
{
	using System;
	using System.IO;
	using System.Security.Cryptography;

	public class EncryptionService
	{
		private readonly byte[] key;
		private readonly byte[] iv;


		public EncryptionService(byte[] key, byte[] iv)
		{
			this.key = key ?? throw new ArgumentNullException(nameof(key));
			this.iv = iv ?? throw new ArgumentNullException(nameof(iv));

			ValidateKeyAndIVLength();
		}

		private void ValidateKeyAndIVLength()
		{
			if (key.Length != 32)
				throw new ArgumentException("Key should be 32 bytes for AES-256.", nameof(key));

			if (iv.Length != 16)
				throw new ArgumentException("IV should be 16 bytes for AES.", nameof(iv));
		}

		public string Encrypt(string plainText)
		{
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = key;
				aesAlg.IV = iv;

				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				using (MemoryStream msEncrypt = new())
				{
					using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
					using (StreamWriter swEncrypt = new(csEncrypt))
					{
						swEncrypt.Write(plainText);
					}

					return Convert.ToBase64String(msEncrypt.ToArray());
				}
			}
		}

		public string Decrypt(string cipherText)
		{
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = key;
				aesAlg.IV = iv;

				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				using (MemoryStream msDecrypt = new(Convert.FromBase64String(cipherText)))
				{
					using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read))
					using (StreamReader srDecrypt = new(csDecrypt))
					{
						return srDecrypt.ReadToEnd();
					}
				}
			}
		}

		public static byte[] GenerateRandomKey(int keySizeInBytes = 32)
		{
			byte[] keyBytes = new byte[keySizeInBytes];

			using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(keyBytes);
			}

			return keyBytes;
		}

		public static byte[] GenerateRandomIV(int ivSizeInBytes = 16)
		{
			byte[] ivBytes = new byte[ivSizeInBytes];

			using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(ivBytes);
			}

			return ivBytes;
		}

	}
}


