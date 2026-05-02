namespace PTTCrawler.Models
{
    public class Post
    {
        public string  Id          { get; set; } = string.Empty; // PTT post ID
        public string? AuthorId    { get; set; }
        public string? AuthorNick  { get; set; }
        public string? Board       { get; set; }
        public string? Title       { get; set; }
        public string? PostTime    { get; set; }
        public string? Content     { get; set; }
        public DateTime CrawledAt  { get; set; }
    }
}
