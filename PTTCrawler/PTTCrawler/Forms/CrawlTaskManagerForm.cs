using PTTCrawler.Business;
using PTTCrawler.Data;
using PTTCrawler.Logging;
using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class CrawlTaskManagerForm : Form
    {
        private readonly DatabaseManager     _db;
        private CancellationTokenSource?     _cts;
        private List<CrawlTask>              _tasks = new();

        public CrawlTaskManagerForm(DatabaseManager db)
        {
            InitializeComponent();
            _db = db;
            LoadTasks();
        }

        // ── 任務載入 ──────────────────────────────────────────

        private void LoadTasks()
        {
            _tasks = _db.GetAllTasks();
            dgvTasks.DataSource = null;
            dgvTasks.DataSource = _tasks.Select(t => new
            {
                t.Id,
                任務名稱   = t.Name,
                性質       = TaskTypeToDisplay(t.TaskType),
                看版網址   = t.BoardUrl,
                關鍵字     = t.Keyword ?? "（全爬）",
                頁數上限   = t.MaxPages.HasValue ? t.MaxPages.Value.ToString() : "不限",
                建立時間   = t.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                狀態       = t.Status == "Active" ? "啟用" : "已放棄"
            }).ToList();

            // 隱藏 Id 欄
            if (dgvTasks.Columns.Contains("Id"))
                dgvTasks.Columns["Id"]!.Visible = false;

            SetButtonState(false);
        }

        private CrawlTask? GetSelectedTask()
        {
            if (dgvTasks.SelectedRows.Count == 0) return null;
            var row = dgvTasks.SelectedRows[0];
            int id  = (int)row.Cells["Id"].Value;
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        // ── 按鈕事件 ──────────────────────────────────────────

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using var dlg = new CrawlTaskEditForm();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var task      = dlg.Result;
            task.CreatedAt = DateTime.Now;
            task.Id        = _db.InsertTask(task);
            AppLogger.Info($"已新增爬蟲任務「{task.Name}」。");
            LoadTasks();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) return;

            using var dlg = new CrawlTaskEditForm(task);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var updated    = dlg.Result;
            updated.Id     = task.Id;
            updated.CreatedAt = task.CreatedAt;
            _db.UpdateTask(updated);
            AppLogger.Info($"已更新爬蟲任務「{updated.Name}」。");
            LoadTasks();
        }

        private void btnAbandon_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) return;

            if (MessageBox.Show($"確定要放棄任務「{task.Name}」嗎？\n（資料會保留，但任務將標記為已放棄）",
                "確認放棄", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _db.AbandonTask(task.Id);
            AppLogger.Info($"已放棄爬蟲任務「{task.Name}」。");
            LoadTasks();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) return;

            if (MessageBox.Show($"確定要刪除任務「{task.Name}」嗎？\n（此操作將一併刪除所有執行記錄，且無法復原）",
                "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            _db.DeleteTask(task.Id);
            AppLogger.Info($"已刪除爬蟲任務「{task.Name}」。");
            LoadTasks();
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) return;

            if (task.Status == "Abandoned")
            {
                MessageBox.Show("此任務已被放棄，無法執行。", "無法執行",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetButtonState(true);
            progressBar.Value = 0;
            lblCurrentPost.Text = "正在初始化…";
            AppLogger.Info($"開始執行爬蟲任務「{task.Name}」。");

            var progress = new Progress<CrawlProgressInfo>(p =>
            {
                lblCurrentPost.Text = $"第 {p.PagesDone + 1} 頁　新增：{p.NewPosts}　略過：{p.Skipped}　目前：{p.CurrentTitle}";
                AppLogger.Debug($"[爬蟲] 新增 {p.NewPosts}　略過 {p.Skipped}　目前：{p.CurrentTitle}");
            });

            _cts = new CancellationTokenSource();
            var crawler = new PttCrawler(_db);

            try
            {
                await crawler.RunAsync(task, progress, _cts.Token);
                lblCurrentPost.Text = "執行完成。";
                AppLogger.Info($"爬蟲任務「{task.Name}」執行完成。");
            }
            catch (OperationCanceledException)
            {
                lblCurrentPost.Text = "已取消。";
                AppLogger.Info($"爬蟲任務「{task.Name}」已取消。");
            }
            catch (Exception ex)
            {
                lblCurrentPost.Text = "執行時發生錯誤。";
                AppLogger.Error($"爬蟲任務「{task.Name}」執行時發生錯誤", ex);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                SetButtonState(false);
                progressBar.Value = 0;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            AppLogger.Info("使用者取消爬蟲任務。");
        }

        private void dgvTasks_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvTasks.SelectedRows.Count > 0;
            btnEdit.Enabled    = hasSelection;
            btnAbandon.Enabled = hasSelection;
            btnDelete.Enabled  = hasSelection;
            btnRun.Enabled     = hasSelection && _cts == null;
        }

        private void SetButtonState(bool isRunning)
        {
            btnAdd.Enabled     = !isRunning;
            btnEdit.Enabled    = !isRunning;
            btnAbandon.Enabled = !isRunning;
            btnDelete.Enabled  = !isRunning;
            btnRun.Enabled     = !isRunning && dgvTasks.SelectedRows.Count > 0;
            btnCancel.Enabled  = isRunning;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            base.OnFormClosing(e);
        }

        private static string TaskTypeToDisplay(CrawlTaskType type) => type switch
        {
            CrawlTaskType.CollectLineId => "收集 Line ID",
            _                           => type.ToString()
        };
    }
}
