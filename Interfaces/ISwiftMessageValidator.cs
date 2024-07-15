using SwiftAPI.Models;

namespace SwiftAPI.Interfaces
{
    public interface ISwiftMessageValidator
    {
        void ValidateMT799(MT799 mt799);
    }
}
