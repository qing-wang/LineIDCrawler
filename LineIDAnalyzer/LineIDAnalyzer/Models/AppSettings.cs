namespace LineIDAnalyzer.Models
{
    /// <summary>
    /// 應用程式設定模型。
    /// </summary>
    public class AppSettings
    {
        /// <summary>ChatGPT API Key（明文；儲存至資料庫時需加密）。</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>使用的 OpenAI 模型名稱，預設為 gpt-4o-mini。</summary>
        public string ModelName { get; set; } = "gpt-4o-mini";

        /// <summary>API 請求逾時秒數，預設 60 秒。</summary>
        public int TimeoutSeconds { get; set; } = 60;
    }
}
