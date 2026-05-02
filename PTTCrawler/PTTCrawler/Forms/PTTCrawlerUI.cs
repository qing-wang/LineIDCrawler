using PTTCrawler.Data;
using PTTCrawler.Logging;

namespace PTTCrawler.Forms
{
    public partial class PTTCrawlerUI : Form
    {
        private readonly DatabaseManager _db;
        private CrawlTaskManagerForm?    _taskMgrForm;
        private PostBrowserForm?         _postBrowserForm;

        public PTTCrawlerUI()
        {
            InitializeComponent();
            _db = new DatabaseManager();
            AppLogger.Initialize(tbConsoleLog);
            AppLogger.Info("PTT Crawler 啟動。");
        }

        private void btnCrawlTasks_Click(object sender, EventArgs e)
        {
            if (_taskMgrForm == null || _taskMgrForm.IsDisposed)
            {
                _taskMgrForm = new CrawlTaskManagerForm(_db);
                _taskMgrForm.Show(this);
            }
            else
            {
                _taskMgrForm.Focus();
            }
        }

        private void btnViewPosts_Click(object sender, EventArgs e)
        {
            if (_postBrowserForm == null || _postBrowserForm.IsDisposed)
            {
                _postBrowserForm = new PostBrowserForm(_db);
                _postBrowserForm.Show(this);
            }
            else
            {
                _postBrowserForm.Focus();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using var dlg = new SettingsForm(_db);
            dlg.ShowDialog(this);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AppLogger.Info("PTT Crawler 關閉。");
            _taskMgrForm?.Close();
            _postBrowserForm?.Close();
            _db.Dispose();
            base.OnFormClosing(e);
        }
    }
}
