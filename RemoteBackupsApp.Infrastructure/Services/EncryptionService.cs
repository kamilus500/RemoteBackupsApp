using RemoteBackupsApp.Domain.ViewModels.Encryption;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using System.Security.Cryptography;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class EncryptionService : IEncryptionService
    {
        public byte[] Decrypt(byte[] encryptedFileBytes, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (MemoryStream msDecrypt = new MemoryStream())
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                {
                    csDecrypt.Write(encryptedFileBytes, 0, encryptedFileBytes.Length);
                    csDecrypt.FlushFinalBlock();
                    return msDecrypt.ToArray();
                }
            }
        }

        public EncryptionViewModel Encrypt(byte[] fileBytes)
        {
            using (Aes aesAlg = Aes.Create())
            using (MemoryStream msEncrypt = new MemoryStream())
            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                    csEncrypt.FlushFinalBlock();
                }

                return new EncryptionViewModel()
                {
                    Content = msEncrypt.ToArray(),
                    AesKey = aesAlg.Key,
                    AesIv = aesAlg.IV
                };
            }
        }
    }
}
