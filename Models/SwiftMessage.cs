namespace SwiftAPI.Models
{
    public class SwiftMessage
    {
        public int Id { get; set; }
        public string BasicHeader { get; set; } = string.Empty; // {1: } mandatory
        public string? ApplicationHeader { get; set; } // {2: } optional
        public string? UserHeader { get; set; } // {3: } optional
        public string? TextBlock { get; set; } // {4: } optional
        public string? Trailer { get; set; } // {5: } optional
        public DateTime CreatedAt { get; set; } // Database will handle default value
    }
}
