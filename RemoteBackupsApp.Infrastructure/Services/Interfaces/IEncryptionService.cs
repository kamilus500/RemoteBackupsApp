using RemoteBackupsApp.Domain.ViewModels.Encryption;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IEncryptionService
    {
        public EncryptionViewModel Encrypt(string tempFilePath);

        public byte[] Decrypt(byte[] encryptedFileBytes, byte[] key, byte[] iv);
    }
}
