using LineIDAnalyzer.Forms;
using NLog;

namespace LineIDAnalyzer;

static class Program
{
    [STAThread]
    static void Main()
    {
        // 確保 logs 目錄存在
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDir);

        ApplicationConfiguration.Initialize();

        Application.ThreadException += (_, e) =>
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Error(e.Exception, "未處理的 UI 執行緒例外");
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Fatal(e.ExceptionObject as Exception, "未處理的應用程式網域例外");
        };

        Application.Run(new LineIDAnalyzerUI());

        LogManager.Shutdown();
    }
}