using PTTCrawler.Data;
using PTTCrawler.Logging;

namespace PTTCrawler.Forms
{
    public partial class PTTCrawlerUI : Form
    {
        private readonly DatabaseManager _db;
        private CrawlTaskManagerForm?    _taskMgrForm;

        public PTTCrawlerUI()
        {
            InitializeComponent();
            _db = new DatabaseManager();
            AppLogger.Initialize(tbConsoleLog);
            AppLogger.Info("PTT Crawler 啟動。");
        }

        private void btnCrawlTasks_Click(object sender, EventArgs e)
        {
            // modeless 單例：若已開啟則聚焦
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AppLogger.Info("PTT Crawler 關閉。");
            _taskMgrForm?.Close();
            _db.Dispose();
            base.OnFormClosing(e);
        }
    }
}
