namespace PTTCrawler.Models
{
    public class AuthorProfileRequest
    {
        public string? Title    { get; set; }
        public string? AuthorId { get; set; }
        public string? Nickname { get; set; }
        public string  Body     { get; set; } = string.Empty;
    }
}
