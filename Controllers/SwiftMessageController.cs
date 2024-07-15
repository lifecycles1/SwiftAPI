using Microsoft.AspNetCore.Mvc;
using SwiftAPI.Interfaces;
using SwiftAPI.Models;

namespace SwiftAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SwiftMessageController : ControllerBase
    {
        private readonly ILogger<SwiftMessageController> _logger;
        private readonly ISwiftMessageService _messageService;
        private const long MaxFileSizeInBytes = 250 * 1024 * 1024; // 250 MB

        public SwiftMessageController(ILogger<SwiftMessageController> logger, ISwiftMessageService messageService)
        {
            _logger = logger;
            _messageService = messageService;
        }

        [HttpPost("mt799")]
        public async Task<IActionResult> AddMT799(IFormFile file)
        {
            // Check if the file is null or empty
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File upload failed: No file provided.");
                return BadRequest("Invalid MT799 message: No file provided.");
            }

            // Restrict the file size to 250 MB
            if (file.Length > MaxFileSizeInBytes)
            {
                _logger.LogWarning("File upload failed: File size exceeds the limit.");
                return BadRequest("Invalid MT799 message: File size exceeds the limit.");
            }

            // Restrict the file format to .txt
            if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("File upload failed: Invalid file format.");
                return BadRequest("Invalid MT799 message: Invalid file format. Only .txt files are supported.");
            }

            _logger.LogInformation("File received successfully: FileName: {FileName}, Size: {FileSize}", file.FileName, file.Length);

            string message;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                message = await reader.ReadToEndAsync();
            }

            try
            {
                // Run the service method
                var swiftMessageResponse = await _messageService.AddMT799(message);

                if (swiftMessageResponse == null)
                {
                    _logger.LogError("Failed to add MT799 message: Service returned null.");
                    return BadRequest("Failed to add MT799 message: Service returned null.");
                }

                return CreatedAtAction(nameof(GetMT799), new { id = swiftMessageResponse.MTMessage.Id }, swiftMessageResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add MT799 message.");
                return BadRequest("Failed to add MT799 message.");
            }
        }

        [HttpGet("mt799/{id}")]
        public async Task<IActionResult> GetMT799(int id)
        {
            try
            {
                var response = await _messageService.GetMT799(id);

                if (response == null)
                {
                    _logger.LogWarning("MT799 record not found with ID: {MessageId}", id);
                    // Return 404 Not Found if no data is found
                    return NotFound(new { Message = "MT799 record not found." });
                }

                _logger.LogInformation("MT799 record found with ID: {MessageId}", id);
                // Return 200 OK with the data
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve MT799 message with ID: {MessageId}", id);
                return BadRequest("Failed to retrieve MT799 message.");
            }
            
        }
    }
}
