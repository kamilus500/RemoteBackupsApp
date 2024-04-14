using RemoteBackupsApp.Domain.ViewModels.Encryption;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IEncryptionService
    {
        public EncryptionViewModel Encrypt(byte[] fileBytes);

        public byte[] Decrypt(byte[] encryptedFileBytes, byte[] key, byte[] iv);
    }
}
