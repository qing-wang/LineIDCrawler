using NLog;
using NLog.Config;
using NLog.Targets;
using System.Drawing;
using System.Windows.Forms;

namespace PTTCrawler.Logging
{
    public enum AppLogLevel { Info, Error, Debug }

    public static class AppLogger
    {
        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        private static RichTextBox?         _console;

        public static void Initialize(RichTextBox consoleLog)
        {
            _console = consoleLog;
        }

        public static void Info(string message)  => Log(AppLogLevel.Info,  message, null);
        public static void Error(string message, Exception? ex = null) => Log(AppLogLevel.Error, message, ex);
        public static void Debug(string message) => Log(AppLogLevel.Debug, message, null);

        private static void Log(AppLogLevel level, string message, Exception? ex)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var levelStr  = level switch
            {
                AppLogLevel.Info  => "資訊",
                AppLogLevel.Error => "錯誤",
                AppLogLevel.Debug => "除錯",
                _                 => "資訊"
            };
            var line = $"[{timestamp}] [{levelStr}] {message}";

            // NLog
            switch (level)
            {
                case AppLogLevel.Info:  _logger.Info(ex, message);  break;
                case AppLogLevel.Error: _logger.Error(ex, message); break;
                case AppLogLevel.Debug: _logger.Debug(ex, message); break;
            }

            // RichTextBox
            AppendToConsole(line, level);
        }

        private static void AppendToConsole(string line, AppLogLevel level)
        {
            if (_console == null) return;

            var color = level switch
            {
                AppLogLevel.Error => Color.Red,
                AppLogLevel.Debug => Color.Gray,
                _                 => Color.Black
            };

            void Append()
            {
                _console!.SelectionStart  = _console.TextLength;
                _console.SelectionLength = 0;
                _console.SelectionColor  = color;
                _console.AppendText(line + Environment.NewLine);
                _console.SelectionColor  = _console.ForeColor;
                _console.ScrollToCaret();
            }

            if (_console.InvokeRequired)
                _console.Invoke(Append);
            else
                Append();
        }
    }
}
