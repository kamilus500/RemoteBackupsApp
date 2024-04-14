using RemoteBackupsApp.Domain.ViewModels.Encryption;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using System.IO.Pipelines;
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
                    byte[] decryptedArray = msDecrypt.ToArray();

                    return decryptedArray;
                }
            }
        }

        public EncryptionViewModel Encrypt(FileStream fileStream)
        {
            using (Aes aesAlg = Aes.Create())
            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            using (CryptoStream csEncrypt = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
            {
                byte[] encryptedData = new byte[fileStream.Length];
                fileStream.Read(encryptedData, 0, encryptedData.Length);

                return new EncryptionViewModel()
                {
                    Content = encryptedData,
                    AesKey = aesAlg.Key,
                    AesIv = aesAlg.IV
                };
            }
        }
    }
}
