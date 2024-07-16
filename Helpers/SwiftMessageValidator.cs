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

        private static readonly Dictionary<string, HashSet<char>> CharacterSets = new Dictionary<string, HashSet<char>>
        {
            { "x", new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/-?:().,'+ \r\n".ToCharArray()) },
            // { "y", new HashSet<char>("...".ToCharArray()) },
            // { "z", new HashSet<char>("...".ToCharArray()) },
        };

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
            ValidateCharacterSet(field20, "x", "20");
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
            ValidateCharacterSet(field21, "x", "21");
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
                ValidateCharacterSet(field, "x", "79");
            }
        }

        private void ValidateCharacterSet(string field, string characterSetKey, string fieldName = "")
        {
          if (!CharacterSets.TryGetValue(characterSetKey, out var characterSet))
          {
              _logger.LogError("Invalid character set type: {CharacterSetKey} for field {FieldName}.", characterSetKey, fieldName);
              throw new Exception($"Invalid character set type: {characterSetKey} for field {fieldName}.");
          }

          for (int position = 0; position < field.Length; position++)
          {
              char c = field[position];
              if (!characterSet.Contains(c))
              {
                  _logger.LogError("Invalid character '{c}' at position {Position} in field {FieldName}.", c, position, fieldName);
                  throw new Exception($"Invalid character '{c}' at position {position} in field {fieldName}.");
              }
          }
        }
    }
}
