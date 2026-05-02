namespace PTTCrawler.Models
{
    public class CrawlExecution
    {
        public int       Id            { get; set; }
        public int       TaskId        { get; set; }
        public DateTime  StartedAt     { get; set; }
        public DateTime? FinishedAt    { get; set; }
        public int       NewPostCount  { get; set; }
        public int       SkippedCount  { get; set; }
    }
}
