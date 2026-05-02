namespace PTTCrawler.Models
{
    public class AppSettings
    {
        public string ApiKey         { get; set; } = string.Empty;
        public string ModelName      { get; set; } = "gpt-4o-mini";
        public int    TimeoutSeconds { get; set; } = 60;
    }
}
