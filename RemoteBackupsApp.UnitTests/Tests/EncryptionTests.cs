namespace RemoteBackupsApp.UnitTests.Tests
{
    public class EncryptionTests
    {
        [Fact]
        public void Encrypt_ReturnEncryptViewModel()
        {
            var bytes = new byte[]
            {
                121,
                09,
                26,
                87
            };

            var encryptionService = new EncryptionService();

            var result = encryptionService.Encrypt(bytes);

            Assert.NotNull(result);
            Assert.Null(result.BackupName);
        }

        [Fact]
        public void Decrypt_ReturnEncryptViewModel()
        {
            var bytes = new byte[]
            {
                01,
                05,
                99,
                127
            };

            var encryptionService = new EncryptionService();

            var encryptionViewModel = encryptionService.Encrypt(bytes);

            var result = encryptionService.Decrypt(encryptionViewModel.Content, encryptionViewModel.AesKey, encryptionViewModel.AesIv);

            Assert.NotNull(result);
        }
    }
}
