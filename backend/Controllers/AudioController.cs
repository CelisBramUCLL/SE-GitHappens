using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AudioController> _logger;

        public AudioController(IWebHostEnvironment environment, ILogger<AudioController> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("stream/{**filePath}")]
        public IActionResult StreamAudio(string filePath)
        {
            try
            {
                // Decode the file path (in case it has special characters)
                filePath = Uri.UnescapeDataString(filePath);

                // Construct the full path
                var fullPath = Path.Combine(_environment.ContentRootPath, filePath);

                _logger.LogInformation("Attempting to stream audio file: {FilePath}", fullPath);

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("Audio file not found: {FilePath}", fullPath);
                    return NotFound(new { error = "Audio file not found", path = filePath });
                }

                // Get the content type
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fullPath, out var contentType))
                {
                    contentType = "audio/mpeg";
                }

                // Stream the file with support for range requests (seeking)
                var stream = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );

                return File(stream, contentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming audio file: {FilePath}", filePath);
                return StatusCode(
                    500,
                    new { error = "Error streaming audio file", message = ex.Message }
                );
            }
        }
    }
}
