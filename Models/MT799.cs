namespace SwiftAPI.Models
{
    // Documentary credits and guarantees
    public class MT799 : MTMessage
    {
        // Transaction Reference Number (Mandatory)
        public string Field20 { get; set; } = string.Empty;
        // Related Reference (Optional)
        public string? Field21 { get; set; }
        // Narrative (Mandatory and can occur more than once)
        public string Field79 { get; set; } = string.Empty;
    }
}
