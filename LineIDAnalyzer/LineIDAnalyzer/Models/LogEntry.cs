namespace LineIDAnalyzer.Models
{
    /// <summary>
    /// 日誌等級列舉。
    /// </summary>
    public enum LogLevel
    {
        /// <summary>資訊</summary>
        Info,
        /// <summary>錯誤</summary>
        Error,
        /// <summary>除錯</summary>
        Debug
    }

    /// <summary>
    /// 單筆日誌條目。
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ExceptionDetail { get; set; }

        public override string ToString()
        {
            var levelText = Level switch
            {
                LogLevel.Info  => "資訊",
                LogLevel.Error => "錯誤",
                LogLevel.Debug => "除錯",
                _              => "資訊"
            };
            var base_ = $"[{Timestamp:HH:mm:ss}] [{levelText}] {Message}";
            return ExceptionDetail != null ? $"{base_}\n{ExceptionDetail}" : base_;
        }
    }
}
