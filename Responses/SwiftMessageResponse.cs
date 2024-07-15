using SwiftAPI.Models;

namespace SwiftAPI.Responses
{
    public class SwiftMessageResponse<T> where T : MTMessage
    {
        public int Id { get; set; }
        public string BasicHeader { get; set; } = string.Empty;
        public string? ApplicationHeader { get; set; }
        public string? UserHeader { get; set; }
        public T MTMessage { get; set; } = null!;
        public string? Trailer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
