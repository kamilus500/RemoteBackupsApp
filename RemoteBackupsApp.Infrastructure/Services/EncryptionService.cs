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
                using (MemoryStream msDecrypt = new MemoryStream(encryptedFileBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    return msDecrypt.ToArray();
                }
            }
        }

        public EncryptionViewModel Encrypt(string tempPathFile)
        {
            var fileBytes = File.ReadAllBytes(tempPathFile);

            using (Aes aesAlg = Aes.Create())
            using (MemoryStream msEncrypt = new MemoryStream(fileBytes))
            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    fileBytes = msEncrypt.ToArray();
                }

                return new EncryptionViewModel()
                {
                    Content = fileBytes,
                    AesKey = aesAlg.Key,
                    AesIv = aesAlg.IV
                };
            }
        }
    }
}
