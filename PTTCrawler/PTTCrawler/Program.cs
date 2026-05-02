using NLog;
using PTTCrawler.Forms;

namespace PTTCrawler;

static class Program
{
    [STAThread]
    static void Main()
    {
        // 全域例外處理
        Application.ThreadException += (s, e) =>
        {
            LogManager.GetCurrentClassLogger().Error(e.Exception, "未處理的執行緒例外");
            MessageBox.Show($"發生未預期的錯誤：{e.Exception.Message}", "錯誤",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            LogManager.GetCurrentClassLogger().Fatal(e.ExceptionObject as Exception, "未處理的應用程式例外");
        };

        ApplicationConfiguration.Initialize();
        Application.Run(new PTTCrawlerUI());
    }
}