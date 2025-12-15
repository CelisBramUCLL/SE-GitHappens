using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dotnet_test.Controllers;
using Microsoft.AspNetCore.Hosting;          // ✅ this provides IWebHostEnvironment
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Unit.Controllers
{
    public class AudioControllerTests
    {
        private readonly string _tempDir;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly Mock<ILogger<AudioController>> _loggerMock;
        private readonly AudioController _controller;

        public AudioControllerTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "AudioTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDir);

            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);

            _loggerMock = new Mock<ILogger<AudioController>>();
            _controller = new AudioController(_envMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenEnvIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioController(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioController(_envMock.Object, null!));
        }

        [Fact]
        public void StreamAudio_ShouldReturnNotFound_WhenFileMissing()
        {
            var result = _controller.StreamAudio("doesnotexist.mp3");

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var body = Assert.IsType<Dictionary<string, object>>(notFound.Value
                .GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(notFound.Value)!));

            Assert.True(body.ContainsKey("error"));
        }

        [Fact]
        public void StreamAudio_ShouldReturnFileResult_WhenFileExists()
        {
            var filePath = Path.Combine(_tempDir, "test.mp3");
            File.WriteAllText(filePath, "dummy data");

            var result = _controller.StreamAudio("test.mp3");

            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("audio/mpeg", fileResult.ContentType);
        }

        [Fact]
        public void StreamAudio_ShouldReturnFileResult_WithDefaultType_WhenUnknownExtension()
        {
            var filePath = Path.Combine(_tempDir, "test.unknownext");
            File.WriteAllText(filePath, "data");

            var result = _controller.StreamAudio("test.unknownext");

            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("audio/mpeg", fileResult.ContentType);
        }
        [Fact]
        public void StreamAudio_ShouldReturn500_WhenExceptionThrown()
        {
            // Pretend that ContentRootPath points to something invalid.
            // This forces Path.Combine() + File.Exists() or FileStream to explode.
            var badEnv = new Mock<IWebHostEnvironment>();
            badEnv.Setup(e => e.ContentRootPath).Throws(new IOException("Boom!"));
            var controller = new AudioController(badEnv.Object, _loggerMock.Object);
            var result = controller.StreamAudio("anypath");
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
    }
}