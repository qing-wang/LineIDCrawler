namespace PTTCrawler.Models
{
    public class CrawlTask
    {
        public int           Id        { get; set; }
        public string        Name      { get; set; } = string.Empty;
        public CrawlTaskType TaskType  { get; set; } = CrawlTaskType.CollectLineId;
        public string        BoardUrl  { get; set; } = string.Empty;
        public string?       Keyword   { get; set; }   // null = 全爬
        public int?          MaxPages  { get; set; }   // null = 不限
        public string        Status    { get; set; } = "Active"; // Active / Abandoned
        public DateTime      CreatedAt { get; set; }
    }
}
