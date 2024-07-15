using SwiftAPI.Helpers;
using SwiftAPI.Interfaces;
using SwiftAPI.Models;
using SwiftAPI.Responses;


namespace SwiftAPI.Services
{
    public class SwiftMessageService : ISwiftMessageService
    {
        private readonly ILogger<SwiftMessageService> _logger;
        private readonly ISwiftMessageParser _parser;
        private readonly ISwiftMessageValidator _validator;
        private readonly ISwiftMessageRepository _repository;

        public SwiftMessageService(ILogger<SwiftMessageService> logger, ISwiftMessageParser parser, ISwiftMessageValidator validator, ISwiftMessageRepository repository)
        {
            _logger = logger;
            _parser = parser;
            _validator = validator;
            _repository = repository;
        }

        public async Task<SwiftMessageResponse<MT799>?> AddMT799(string message)
        {
            // parse the message
            var (swiftMessage, mt799) = _parser.ParseMT799(message);

            if (swiftMessage == null) {
                _logger.LogError("Failed to parse the SWIFT message.");
                return null;
            }

            if (mt799 == null)
            {
                _logger.LogError("Failed to parse the MT799 message.");
                return null;
            }

            try
            {
                _validator.ValidateMT799(mt799);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MT799 validation failed.");
                // Validation failed, so do not call repository method. Return null instead.
                return null;
            }

            // If mt799 is successfully validated, call repository method
            try
            {
                var (insertedSwiftMessage, insertedMT799) = await _repository.AddMT799(swiftMessage, mt799);

                if (insertedSwiftMessage == null)
                {
                    _logger.LogError("Failed to insert SWIFT message: Repository returned null.");
                    return null;
                }

                if (insertedMT799 == null)
                {
                    _logger.LogError("Failed to insert MT799 message: Repository returned null.");
                    return null;
                }

                _logger.LogInformation("Logging the inserted MT799's ID: {insertedMT799.Id}, SwiftMessageId: {insertedMT799.SwiftMessageId}", insertedMT799.Id, insertedMT799.SwiftMessageId);

                var response = new SwiftMessageResponse<MT799>
                {
                    Id = insertedSwiftMessage.Id,
                    BasicHeader = insertedSwiftMessage.BasicHeader,
                    ApplicationHeader = insertedSwiftMessage.ApplicationHeader,
                    UserHeader = insertedSwiftMessage.UserHeader,
                    MTMessage = insertedMT799,
                    Trailer = insertedSwiftMessage.Trailer,
                    CreatedAt = insertedSwiftMessage.CreatedAt
                };

                _logger.LogInformation("Returning ADD response with ID: {Id}, MTMessage.Id: {MTMessage.Id}, MTMessage.SwiftMessageId: {MTMessage.SwiftMessageId}", response.Id, response.MTMessage.Id, response.MTMessage.SwiftMessageId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add MT799 message");
                throw;
            }
        }

        public async Task<SwiftMessageResponse<MT799>?> GetMT799(int id)
        {
            _logger.LogInformation("Fetching MT799 message with ID: {id}", id);

            try
            {
                var (swiftMessage, mt799) = await _repository.GetMT799(id);

                if (swiftMessage == null || mt799 == null)
                {
                    _logger.LogError("Failed to retrieve SWIFT message with ID: {id} from repository.", id);
                    return null;
                }

                var response = new SwiftMessageResponse<MT799>
                {
                    Id = swiftMessage.Id,
                    BasicHeader = swiftMessage.BasicHeader,
                    ApplicationHeader = swiftMessage.ApplicationHeader,
                    UserHeader = swiftMessage.UserHeader,
                    MTMessage = mt799,
                    Trailer = swiftMessage.Trailer,
                    CreatedAt = swiftMessage.CreatedAt
                };

                _logger.LogInformation("Returning GET response with ID: {Id}, MTMessage.Id: {MTMessage.Id}, MTMessage.SwiftMessageId: {MTMessage.SwiftMessageId}", response.Id, response.MTMessage.Id, response.MTMessage.SwiftMessageId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve MT799 message with ID: {id}", id);
                throw;
            }
        }
    }
}
