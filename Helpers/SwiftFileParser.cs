using SwiftAPI.Interfaces;

namespace SwiftAPI.Helpers
{
    public class SwiftFileParser : ISwiftFileParser
    {
        private const long MaxFileSizeInBytes = 250 * 1024 * 1024; // 250 MB
        private readonly ILogger<SwiftFileParser> _logger;

        public SwiftFileParser(ILogger<SwiftFileParser> logger)
        {
            _logger = logger;
        }

        public async Task<string> ParseFile(IFormFile file)
        {
            // Check if the file is null or empty
            if (file == null || file.Length == 0)
            {
                _logger.LogError("File upload failed: No file provided.");
                throw new ArgumentException("Invalid MT799 message: No file provided.");
            }

            // Restrict the file size to 250 MB
            if (file.Length > MaxFileSizeInBytes)
            {
                _logger.LogError("File upload failed: File size exceeds the limit.");
                throw new ArgumentException("Invalid MT799 message: File size exceeds the limit.");
            }

            // Restrict the file format to .txt
            if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("File upload failed: Invalid file format.");
                throw new ArgumentException("Invalid MT799 message: Invalid file format. Only .txt files are supported.");
            }

            _logger.LogInformation("File received successfully: FileName: {FileName}, Size: {FileSize}", file.FileName, file.Length);

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
