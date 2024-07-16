using SwiftAPI.Models;
using SwiftAPI.Responses;

namespace SwiftAPI.Interfaces
{
    public interface ISwiftMessageService
    {
        Task<SwiftMessageResponse<MT799>> AddMT799(string message);
        Task<SwiftMessageResponse<MT799>> GetMT799(int id);
    }
}
