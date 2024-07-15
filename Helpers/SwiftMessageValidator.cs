using SwiftAPI.Interfaces;
using SwiftAPI.Models;

namespace SwiftAPI.Helpers
{
    public class SwiftMessageValidator : ISwiftMessageValidator
    {
        private readonly ILogger<SwiftMessageValidator> _logger;

        public SwiftMessageValidator(ILogger<SwiftMessageValidator> logger)
        {
            _logger = logger;
        }

        // TO-DO: Implement Validate blocks of Swift Message

        public void ValidateMT799(MT799 mt799)
        {
            try
            {
                ValidateField20(mt799.Field20);
                if (mt799.Field21 != null)
                    ValidateField21(mt799.Field21);
                ValidateField79(mt799.Field79);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MT799 validation failed.");
                throw;
            }
        }

        private void ValidateField20(string field20)
        {
            if (field20.Length > 16)
            {
                _logger.LogError("Field 20 exceeds the maximum length of 16 characters.");
                throw new Exception("Field 20 exceeds the maximum length of 16 characters.");
            }
            if (field20.StartsWith("/") || field20.Contains("//") || field20.EndsWith("/"))
            {
                _logger.LogError("Field 20 cannot start or end with a slash, or contain double slashes.");
                throw new Exception("Field 20 cannot start or end with a slash, or contain double slashes.");
            }
        }

        private void ValidateField21(string field21)
        {
            if (field21.Length > 16)
            {
                _logger.LogError("Field 21 exceeds the maximum length of 16 characters.");
                throw new Exception("Field 21 exceeds the maximum length of 16 characters.");
            }
            if (field21.StartsWith("/") || field21.Contains("//") || field21.EndsWith("/"))
            {
                _logger.LogError("Field 21 cannot start or end with a slash, or contain double slashes.");
                throw new Exception("Field 21 cannot start or end with a slash, or contain double slashes.");
            }
        }

        private void ValidateField79(string field79)
        {
            var field79List = field79.Split("||").ToList();

            foreach (var field in field79List)
            {
                if (field.Length > 35 * 50)
                {
                    _logger.LogError("Field 79 exceeds the maximum length of 35 multilines of 50 characters each.");
                    throw new Exception("Field 79 exceeds the maximum length of 35 multilines of 50 characters each.");
                }
                if (field.Split("\r\n").Length > 35)
                {
                    _logger.LogError("Field 79 exceeds the maximum number of lines (35).");
                    throw new Exception("Field 79 exceeds the maximum number of lines (35).");
                }
                if (field.Split("\r\n").Any(line => line.Length > 50))
                {
                    _logger.LogError("Field 79 exceeds the maximum length of 50 characters per line.");
                    throw new Exception("Field 79 exceeds the maximum length of 50 characters per line.");
                }
            }
        }
    }
}
