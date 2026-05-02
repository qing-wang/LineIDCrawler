using PTTCrawler.Data;
using PTTCrawler.Logging;
using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class PostBrowserForm : Form
    {
        private const int PageSize = 20;

        private readonly DatabaseManager _db;
        private int    _currentPage;
        private int    _totalPages;
        private int    _totalCount;
        private bool   _ascending = true;   // 預設由舊到新

        public PostBrowserForm(DatabaseManager db)
        {
            InitializeComponent();
            _db = db;
            RefreshFilterSources();
        }

        // ── 初始化篩選資料來源 ────────────────────────────────

        private void RefreshFilterSources()
        {
            // 看版清單
            var boards = _db.GetDistinctBoards();
            cbBoards.Items.Clear();
            foreach (var b in boards) cbBoards.Items.Add(b);
            if (cbBoards.Items.Count > 0) cbBoards.SelectedIndex = 0;

            // 任務清單（有貼文的）
            var tasks = _db.GetTasksWithPosts();
            cbTasks.Items.Clear();
            foreach (var t in tasks) cbTasks.Items.Add(new TaskComboItem(t));
            if (cbTasks.Items.Count > 0) cbTasks.SelectedIndex = 0;

            UpdateFilterEnabled();
        }

        // ── 瀏覽方式切換 ─────────────────────────────────────

        private void rbByBoard_CheckedChanged(object sender, EventArgs e) => UpdateFilterEnabled();

        private void UpdateFilterEnabled()
        {
            cbBoards.Enabled = rbByBoard.Checked;
            cbTasks.Enabled  = rbByTask.Checked;
        }

        // ── 載入 / 排序 ───────────────────────────────────────

        private void btnLoad_Click(object sender, EventArgs e)
        {
            _ascending    = rbAscending.Checked;
            _currentPage  = 0;
            LoadTotalCount();
            LoadPage();
        }

        private void rbAscending_CheckedChanged(object sender, EventArgs e)
        {
            if (_totalPages > 0)
            {
                _ascending = rbAscending.Checked;
                LoadPage();
            }
        }

        private void LoadTotalCount()
        {
            if (rbByBoard.Checked)
            {
                if (cbBoards.SelectedItem == null) { _totalCount = 0; _totalPages = 0; return; }
                _totalCount = _db.GetPostCountByBoard(cbBoards.SelectedItem.ToString()!);
            }
            else
            {
                if (cbTasks.SelectedItem == null) { _totalCount = 0; _totalPages = 0; return; }
                var task = ((TaskComboItem)cbTasks.SelectedItem).Task;
                _totalCount = _db.GetPostCountByTask(task.Id);
            }
            _totalPages = Math.Max(1, (int)Math.Ceiling((double)_totalCount / PageSize));
        }

        private void LoadPage()
        {
            List<Post> posts;

            if (rbByBoard.Checked)
            {
                if (cbBoards.SelectedItem == null) return;
                posts = _db.GetPostsByBoard(cbBoards.SelectedItem.ToString()!, _currentPage, PageSize, _ascending);
            }
            else
            {
                if (cbTasks.SelectedItem == null) return;
                var task = ((TaskComboItem)cbTasks.SelectedItem).Task;
                posts = _db.GetPostsByTask(task.Id, _currentPage, PageSize, _ascending);
            }

            dgvPosts.DataSource = posts.Select(p => new
            {
                p.Id,
                標題     = p.Title ?? string.Empty,
                作者     = string.IsNullOrEmpty(p.AuthorNick)
                           ? (p.AuthorId ?? string.Empty)
                           : $"{p.AuthorId} ({p.AuthorNick})",
                看版     = p.Board ?? string.Empty,
                貼文時間 = p.PostTime ?? string.Empty,
                爬取時間 = p.CrawledAt.ToString("yyyy/MM/dd HH:mm")
            }).ToList();

            // 隱藏 Id 欄（供雙擊時取用）
            if (dgvPosts.Columns.Contains("Id"))
                dgvPosts.Columns["Id"]!.Visible = false;

            // 設定各欄相對寬度（標題欄約為其他欄的兩倍）
            SetColumnWeights();

            UpdatePagination();
            AppLogger.Debug($"貼文瀏覽：載入第 {_currentPage + 1}/{_totalPages} 頁，共 {_totalCount} 則。");
        }

        private void SetColumnWeights()
        {
            var weights = new Dictionary<string, float>
            {
                ["標題"]   = 250f,
                ["作者"]   = 120f,
                ["看版"]   = 80f,
                ["貼文時間"] = 110f,
                ["爬取時間"] = 110f,
            };
            foreach (DataGridViewColumn col in dgvPosts.Columns)
            {
                if (weights.TryGetValue(col.Name, out float w))
                    col.FillWeight = w;
            }
        }

        private void UpdatePagination()
        {
            lblPageInfo.Text   = $"第 {_currentPage + 1} / {_totalPages} 頁（共 {_totalCount} 則）";
            btnFirst.Enabled   = _currentPage > 0;
            btnPrev.Enabled    = _currentPage > 0;
            btnNext.Enabled    = _currentPage < _totalPages - 1;
            btnLast.Enabled    = _currentPage < _totalPages - 1;
        }

        // ── 分頁按鈕 ──────────────────────────────────────────

        private void btnFirst_Click(object sender, EventArgs e) { _currentPage = 0;              LoadPage(); }
        private void btnPrev_Click(object sender, EventArgs e)  { _currentPage--;                 LoadPage(); }
        private void btnNext_Click(object sender, EventArgs e)  { _currentPage++;                 LoadPage(); }
        private void btnLast_Click(object sender, EventArgs e)  { _currentPage = _totalPages - 1; LoadPage(); }

        // ── 貼文雙擊 → PostViewForm ───────────────────────────

        private void dgvPosts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var id   = dgvPosts.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
            if (string.IsNullOrEmpty(id)) return;

            var post = _db.GetPost(id);
            if (post == null) return;

            using var form = new PostViewForm(post);
            form.ShowDialog(this);
        }

        // ── 載入時初始化為最末頁 ──────────────────────────────

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (cbBoards.Items.Count > 0 || cbTasks.Items.Count > 0)
            {
                LoadTotalCount();
                // 預設進入時顯示最末頁
                _currentPage = Math.Max(0, _totalPages - 1);
                LoadPage();
            }
        }
    }

    internal class TaskComboItem
    {
        public CrawlTask Task { get; }
        public TaskComboItem(CrawlTask t) => Task = t;
        public override string ToString() =>
            string.IsNullOrEmpty(Task.Keyword)
                ? $"{Task.Name} ({Task.BoardUrl.TrimEnd('/').Split('/').LastOrDefault()})"
                : $"{Task.Name} [{Task.Keyword}]";
    }
}
