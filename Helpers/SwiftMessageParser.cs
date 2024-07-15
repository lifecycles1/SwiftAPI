using SwiftAPI.Interfaces;
using SwiftAPI.Models;

namespace SwiftAPI.Helpers
{
    public class SwiftMessageParser : ISwiftMessageParser
    {
        private readonly ILogger<SwiftMessageParser> _logger;

        public SwiftMessageParser(ILogger<SwiftMessageParser> logger)
        {
            _logger = logger;
        }

        private SwiftMessage BlockParser(string message)
        {
            string? block1 = ExtractField(message, "{1:", "}");
            if (block1 == null)
            {
                _logger.LogError("Invalid SWIFT message: Mandatory Block 1 (BasicHeader) is missing.");
                throw new Exception("Invalid SWIFT message: Mandatory Block 1 (BasicHeader) is missing.");
            }
            string? block2 = ExtractField(message, "{2:", "}");
            string? block3 = ExtractField(message, "{3:", "}}");
            string? block4 = ExtractField(message, "{4:", "}");
            string? block5 = ExtractField(message, "{5:", "}}");

            // Add trailing '}' to block3 and block5 if they are not null
            if (block3 != null)
            {
                block3 = block3.TrimEnd() + "}";
            }
            if (block5 != null)
            {
                block5 = block5.TrimEnd() + "}";
            }

            return new SwiftMessage
            {
                BasicHeader = block1,
                ApplicationHeader = block2,
                UserHeader = block3,
                TextBlock = block4,
                Trailer = block5
            };
        }

        private static string? ExtractField(string message, string startIndicator, string endIndicator)
        {
            int start = message.IndexOf(startIndicator);
            if (start == -1)
                return null;

            start += startIndicator.Length;
            int end = message.IndexOf(endIndicator, start);
            if (end == -1)
                end = message.Length;

            return message[start..end].Trim();
        }

        private static List<string> Extract79Fields(string textBlock)
        {
            var fields = new List<string>();
            int start = 0;
            string startIndicator = ":79:";

            while ((start = textBlock.IndexOf(startIndicator, start)) != -1)
            {
                start += startIndicator.Length;
                int end = textBlock.Length;
                int nextStart = textBlock.IndexOf(startIndicator, start);
                if (nextStart != -1)
                    end = nextStart;
                // extract the field between start and end
                var field = textBlock[start..end].Trim();
                if (!string.IsNullOrEmpty(field))
                    fields.Add(field);
                // move start to the end of the current field
                start = end;
            }
            return fields;
        }

        public (SwiftMessage?, MT799?) ParseMT799(string message)
        {
            SwiftMessage swiftMessage;

            try
            {
               swiftMessage = BlockParser(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse MT799 message: Block parsing failed.");
                throw;
            }

            if (swiftMessage.TextBlock != null)
            {
                var textBlock = swiftMessage.TextBlock;
                var field20 = ExtractField(textBlock, ":20:", "\r\n");
                if (field20 == null)
                {
                    _logger.LogError("Invalid MT799 message: Mandatory Field 20 is missing.");
                    throw new Exception("Invalid MT799 message: Mandatory Field 20 is missing.");
                }
                var field21 = ExtractField(textBlock, ":21:", "\r\n");
                var field79List = Extract79Fields(textBlock);
                if (field79List.Count == 0)
                {
                    _logger.LogError("Invalid MT799 message: Mandatory Field 79 is missing.");
                    throw new Exception("Invalid MT799 message: Mandatory Field 79 is missing.");
                }

                var field79 = string.Join("||", field79List);

                return (swiftMessage, new MT799
                {
                    Field20 = field20,
                    Field21 = field21,
                    Field79 = field79
                });
            }
            else
            {
                _logger.LogError("Failed to parse MT799 message: TextBlock is missing.");
                throw new Exception("Failed to parse MT799 message: TextBlock is missing.");
            }
        }
    }
}
