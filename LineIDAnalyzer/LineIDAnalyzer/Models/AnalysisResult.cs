namespace LineIDAnalyzer.Models
{
    /// <summary>
    /// 代表一次 Line ID 分析的結果。
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>輸入文字是否含有 Line ID。</summary>
        public bool HasLineId { get; set; }

        /// <summary>從輸入文字中萃取出的所有 Line ID。</summary>
        public List<string> ExtractedIds { get; set; } = new();

        /// <summary>LLM 原始回應文字。</summary>
        public string RawResponse { get; set; } = string.Empty;

        /// <summary>分析時發生的錯誤訊息（無錯誤時為 null）。</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>分析是否成功完成（無例外、無取消）。</summary>
        public bool IsSuccess => ErrorMessage == null;
    }
}
