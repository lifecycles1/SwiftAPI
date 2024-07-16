using SwiftAPI.Models;

namespace SwiftAPI.Interfaces
{
    public interface ISwiftMessageParser
    {
        (SwiftMessage, MT799) ParseMT799(string message);
    }
}
