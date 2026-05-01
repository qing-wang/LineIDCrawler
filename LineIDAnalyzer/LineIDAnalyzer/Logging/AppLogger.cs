using NLog;
using RichTextBoxLogLevel = LineIDAnalyzer.Models.LogLevel;

namespace LineIDAnalyzer.Logging
{
    /// <summary>
    /// 封裝 NLog，同時將日誌輸出至：
    ///   1. 每日滾動的日誌檔案（透過 NLog）
    ///   2. 主表單的 RichTextBox（依等級顯示不同顏色）
    /// </summary>
    public static class AppLogger
    {
        private static readonly Logger _nlog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 綁定的 RichTextBox（由主表單在初始化時設定）。
        /// </summary>
        public static RichTextBox? ConsoleLogBox { get; set; }

        // ── 公開方法 ──────────────────────────────────────────

        public static void Info(string message)
            => Write(RichTextBoxLogLevel.Info, message, null);

        public static void Error(string message, Exception? ex = null)
            => Write(RichTextBoxLogLevel.Error, message, ex);

        public static void Debug(string message)
            => Write(RichTextBoxLogLevel.Debug, message, null);

        // ── 核心寫入 ──────────────────────────────────────────

        private static void Write(RichTextBoxLogLevel level, string message, Exception? ex)
        {
            var exDetail = ex != null
                ? $"例外類型：{ex.GetType().Name}\n訊息：{ex.Message}\n堆疊追蹤：{ex.StackTrace}"
                : null;

            var entry = new Models.LogEntry
            {
                Level          = level,
                Message        = message,
                ExceptionDetail = exDetail
            };

            // 1. 寫入 NLog（檔案）
            switch (level)
            {
                case RichTextBoxLogLevel.Info:
                    _nlog.Info(message);
                    if (ex != null) _nlog.Info(ex, "例外詳情");
                    break;
                case RichTextBoxLogLevel.Error:
                    _nlog.Error(ex, message);
                    break;
                case RichTextBoxLogLevel.Debug:
                    _nlog.Debug(message);
                    break;
            }

            // 2. 寫入 RichTextBox（UI 執行緒安全）
            AppendToConsoleLog(entry);
        }

        private static void AppendToConsoleLog(Models.LogEntry entry)
        {
            if (ConsoleLogBox == null) return;

            var color = entry.Level switch
            {
                RichTextBoxLogLevel.Error => Color.Red,
                RichTextBoxLogLevel.Debug => Color.Gray,
                _                         => Color.Black
            };

            var text = entry.ToString() + Environment.NewLine;

            Action append = () =>
            {
                ConsoleLogBox.SelectionStart  = ConsoleLogBox.TextLength;
                ConsoleLogBox.SelectionLength = 0;
                ConsoleLogBox.SelectionColor  = color;
                ConsoleLogBox.AppendText(text);
                ConsoleLogBox.SelectionColor  = ConsoleLogBox.ForeColor;
                // 自動捲動到最新一行
                ConsoleLogBox.ScrollToCaret();
            };

            if (ConsoleLogBox.InvokeRequired)
                ConsoleLogBox.Invoke(append);
            else
                append();
        }
    }
}
