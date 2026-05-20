using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using TechMove.Web.Services;
using static TechMove.Web.Services.FileValidationService;
using Xunit;

namespace TechMove.Tests
{
    public class FileValidationTests
    {
        private FileValidationService CreateService()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            return new FileValidationService(envMock.Object);
        }

        private IFormFile CreateMockFile(string fileName, long size = 1024)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(size);
            return fileMock.Object;
        }

        [Fact]
        public void Validate_PdfFile_DoesNotThrow()
        {
            var service = CreateService();
            var file = CreateMockFile("signed_agreement.pdf");

            var exception = Record.Exception(() => service.Validate(file));

            Assert.Null(exception);
        }

        [Fact]
        public void Validate_ExeFile_ThrowsInvalidOperationException()
        {
            var service = CreateService();
            var file = CreateMockFile("malware.exe");

            Assert.Throws<InvalidOperationException>(() => service.Validate(file));
        }

        [Fact]
        public void Validate_DocxFile_ThrowsInvalidOperationException()
        {
            var service = CreateService();
            var file = CreateMockFile("contract.docx");

            Assert.Throws<InvalidOperationException>(() => service.Validate(file));
        }

        [Fact]
        public void Validate_ImageFile_ThrowsInvalidOperationException()
        {
            var service = CreateService();
            var file = CreateMockFile("scan.jpg");

            Assert.Throws<InvalidOperationException>(() => service.Validate(file));
        }

        [Fact]
        public void Validate_FileTooLarge_ThrowsInvalidOperationException()
        {
            var service = CreateService();
            var oversizedFile = CreateMockFile("big_contract.pdf", size: 11 * 1024 * 1024); // 11 MB

            Assert.Throws<InvalidOperationException>(() => service.Validate(oversizedFile));
        }

        [Fact]
        public void Validate_FileAtMaxSize_DoesNotThrow()
        {
            var service = CreateService();
            var maxFile = CreateMockFile("max_contract.pdf", size: FileValidationService.MaxFileSizeBytes);

            var exception = Record.Exception(() => service.Validate(maxFile));

            Assert.Null(exception);
        }

        [Fact]
        public void Validate_NullFile_ThrowsArgumentNullException()
        {
            var service = CreateService();

            Assert.Throws<ArgumentNullException>(() => service.Validate(null!));
        }

        [Fact]
        public void Validate_ExeFile_ErrorMessageContainsExtension()
        {
            var service = CreateService();
            var file = CreateMockFile("virus.exe");

            var ex = Assert.Throws<InvalidOperationException>(() => service.Validate(file));

            Assert.Contains(".exe", ex.Message);
        }
    }
}
