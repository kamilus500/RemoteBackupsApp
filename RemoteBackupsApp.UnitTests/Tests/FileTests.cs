using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using RemoteBackupsApp.Infrastructure.Services;

namespace RemoteBackupsApp.UnitTests.Tests
{
    public class FileTests
    {
        [Fact]
        public async Task GetFileContent_ReturnArrayByte()
        {
            var file = CreateSampleFile();

            var fileService = new FileService();

            var result = await fileService.GetFileContent(file);
        
            Assert.NotNull(result);
        }

        private static IFormFile CreateSampleFile()
        {
            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            writer.Write("Przykładowa zawartość pliku.");
            writer.Flush();
            stream.Position = 0;

            var file = new FormFile(stream, 0, stream.Length, "sampleFile", "sample.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            return file;
        }
    }
}
