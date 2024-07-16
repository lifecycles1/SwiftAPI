namespace SwiftAPI.Interfaces
{
    public interface ISwiftFileParser
    {
        Task<string> ParseFile(IFormFile file);
    }
}
