using SwiftAPI.Models;
using SwiftAPI.Responses;

namespace SwiftAPI.Interfaces
{
    public interface ISwiftMessageRepository
    {
        Task<(SwiftMessage?, MT799?)> AddMT799(SwiftMessage swiftMessage, MT799 mt799);
        Task<(SwiftMessage?, MT799?)> GetMT799(int id);
    }
}
