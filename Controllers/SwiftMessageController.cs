using Microsoft.AspNetCore.Mvc;
using SwiftAPI.Interfaces;
using SwiftAPI.Models;
using SwiftAPI.Responses;

namespace SwiftAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SwiftMessageController : ControllerBase
    {
        private readonly ILogger<SwiftMessageController> _logger;
        private readonly ISwiftFileParser _fileParser;
        private readonly ISwiftMessageService _messageService;

        public SwiftMessageController(ILogger<SwiftMessageController> logger, ISwiftFileParser fileParser, ISwiftMessageService messageService)
        {
            _logger = logger;
            _fileParser = fileParser;
            _messageService = messageService;
        }

        [HttpPost("mt799")]
        public async Task<IActionResult> AddMT799(IFormFile file)
        {
            try
            {
                // Check if the file is null or empty, and parse to string
                string message = await _fileParser.ParseFile(file);

                // Run the service method
                SwiftMessageResponse<MT799> swiftMessageResponse = await _messageService.AddMT799(message);

                return CreatedAtAction(nameof(GetMT799), new { id = swiftMessageResponse.MTMessage.Id }, swiftMessageResponse);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to add MT799 message.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add MT799 message.");
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("mt799/{id}")]
        public async Task<IActionResult> GetMT799(int id)
        {
            try
            {
                SwiftMessageResponse<MT799> response = await _messageService.GetMT799(id);

                _logger.LogInformation("MT799 record found with ID: {MessageId}", id);
                // Return 200 OK with the data
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(ex, "MT799 record not found with ID: {MessageId}", id);
                    // Return 404 Not Found if no data is found
                    return NotFound(new { Message = "MT799 record not found." });
                }

                // Log unexpected errors and return 500 Internal Server Error
                _logger.LogError(ex, "An unexpected error occurred while retrieving MT799 record with ID: {MessageId}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }
    }
}
