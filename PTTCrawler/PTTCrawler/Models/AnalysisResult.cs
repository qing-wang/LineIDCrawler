namespace PTTCrawler.Models
{
    public class AnalysisResult
    {
        public bool         HasLineId    { get; set; }
        public List<string> ExtractedIds { get; set; } = new();
        public string       RawResponse  { get; set; } = string.Empty;
        public string?      ErrorMessage { get; set; }
        public bool         IsSuccess    => ErrorMessage == null;
    }
}
