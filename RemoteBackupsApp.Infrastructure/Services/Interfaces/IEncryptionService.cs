using RemoteBackupsApp.Domain.ViewModels;

namespace RemoteBackupsApp.Infrastructure.Services.Interfaces
{
    public interface IEncryptionService
    {
        public EncryptionViewModel Encrypt(byte[] fileBytes);

        public byte[] Decrypt(byte[] encryptedFileBytes, byte[] key, byte[] iv);
    }
}
