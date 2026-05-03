using System.Text;
using PTTCrawler.Business;
using PTTCrawler.Data;
using PTTCrawler.Logging;
using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class AnalyzeLineIdForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly AppSettings     _settings;
        private readonly List<Post>      _currentPagePosts;
        private readonly string?         _boardFilter;
        private readonly int             _taskIdFilter;

        // 並行控制：最多同時 3 個 API 請求
        private static readonly SemaphoreSlim _apiSemaphore = new(3, 3);

        private CancellationTokenSource? _cts;
        private bool _isRunning;

        // 分析結果
        private readonly List<(Post Post, List<string> LineIds)> _withLineId    = new();
        private readonly List<Post>                               _withoutLineId = new();

        public AnalyzeLineIdForm(
            DatabaseManager db,
            AppSettings     settings,
            List<Post>      currentPagePosts,
            string?         boardFilter,
            int             taskIdFilter)
        {
            InitializeComponent();
            _db               = db;
            _settings         = settings;
            _currentPagePosts = currentPagePosts;
            _boardFilter      = boardFilter;
            _taskIdFilter     = taskIdFilter;
        }

        // ── 分析執行 ──────────────────────────────────────────

        private async void btnStartAnalyze_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                _cts?.Cancel();
                return;
            }

            // 取得待分析貼文清單
            List<Post> posts;
            if (rbScopePage.Checked)
            {
                posts = _currentPagePosts;
            }
            else
            {
                try
                {
                    if (_boardFilter != null)
                        posts = _db.GetAllPostsByBoard(_boardFilter, true);
                    else if (_taskIdFilter > 0)
                        posts = _db.GetAllPostsByTask(_taskIdFilter, true);
                    else
                    {
                        MessageBox.Show("無法取得所有貼文（篩選條件未設定）。",
                            "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"載入貼文失敗：{ex.Message}", ex);
                    MessageBox.Show($"載入貼文失敗：{ex.Message}",
                        "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (posts.Count == 0)
            {
                MessageBox.Show("沒有可分析的貼文。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetRunningState(true);
            _withLineId.Clear();
            _withoutLineId.Clear();
            ClearResultGrids();

            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            AppLogger.Info($"開始分析 Line ID，共 {posts.Count} 篇貼文。");

            try
            {
                var analyzer   = new LineIdAnalyzer(_settings);
                int done       = 0;
                int stage1Skip = 0;
                var total      = posts.Count;

                progressBar.Maximum = total;
                progressBar.Value   = 0;

                // 並行分析（使用 SemaphoreSlim 控制並發數）
                var tasks = posts.Select(async post =>
                {
                    await _apiSemaphore.WaitAsync(ct);
                    try
                    {
                        ct.ThrowIfCancellationRequested();

                        UpdateProgress(done, total, post.Title ?? post.Id);
                        var result = await analyzer.AnalyzeAsync(post.Content ?? string.Empty, ct);

                        lock (_withLineId)
                        {
                            if (result.HasLineId)
                                _withLineId.Add((post, result.ExtractedIds));
                            else
                                _withoutLineId.Add(post);

                            if (result.RawResponse.StartsWith("[Stage 1]"))
                                Interlocked.Increment(ref stage1Skip);

                            Interlocked.Increment(ref done);
                        }
                    }
                    finally
                    {
                        _apiSemaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                AppLogger.Info($"分析完成：{_withLineId.Count} 篇含 Line ID，{_withoutLineId.Count} 篇不含，" +
                               $"Stage 1 略過 {stage1Skip} 篇（無需 API 費用）。");
            }
            catch (OperationCanceledException)
            {
                AppLogger.Info("分析已取消。");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"分析時發生錯誤：{ex.Message}", ex);
                MessageBox.Show($"分析時發生錯誤：{ex.Message}",
                    "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetRunningState(false);
                ShowResults();
            }
        }

        // ── 進度更新（執行緒安全）────────────────────────────

        private void UpdateProgress(int done, int total, string? title)
        {
            if (InvokeRequired) { Invoke(() => UpdateProgress(done, total, title)); return; }
            progressBar.Value   = Math.Min(done, total);
            lblProgress.Text    = $"第 {done + 1} / {total} 篇：{title}";
        }

        // ── 結果顯示 ──────────────────────────────────────────

        private void ShowResults()
        {
            if (InvokeRequired) { Invoke(ShowResults); return; }

            progressBar.Value = progressBar.Maximum;
            lblProgress.Text  = $"分析完成：{_withLineId.Count} 篇含 Line ID，{_withoutLineId.Count} 篇不含。";
            btnExport.Enabled = _withLineId.Count > 0;

            // 含 Line ID
            grpWithLineId.Text = $"含 Line ID 的貼文（{_withLineId.Count} 篇）";
            dgvWithLineId.DataSource = _withLineId
                .OrderBy(x => x.Post.Title)
                .Select(x => new
                {
                    x.Post.Id,
                    標題   = x.Post.Title   ?? string.Empty,
                    作者   = x.Post.AuthorId ?? string.Empty,
                    看版   = x.Post.Board    ?? string.Empty,
                    LineID = string.Join(", ", x.LineIds)
                }).ToList();
            HideIdColumn(dgvWithLineId);
            SetColumnWeights(dgvWithLineId, new() { ["標題"] = 250f, ["LineID"] = 150f, ["作者"] = 100f, ["看版"] = 80f });

            // 不含 Line ID
            grpWithoutLineId.Text = $"不含 Line ID 的貼文（{_withoutLineId.Count} 篇）";
            dgvWithoutLineId.DataSource = _withoutLineId
                .OrderBy(p => p.Title)
                .Select(p => new
                {
                    p.Id,
                    標題 = p.Title   ?? string.Empty,
                    作者 = p.AuthorId ?? string.Empty,
                    看版 = p.Board    ?? string.Empty
                }).ToList();
            HideIdColumn(dgvWithoutLineId);
            SetColumnWeights(dgvWithoutLineId, new() { ["標題"] = 300f, ["作者"] = 100f, ["看版"] = 80f });
        }

        private void ClearResultGrids()
        {
            dgvWithLineId.DataSource    = null;
            dgvWithoutLineId.DataSource = null;
            grpWithLineId.Text          = "含 Line ID 的貼文";
            grpWithoutLineId.Text       = "不含 Line ID 的貼文";
        }

        private static void HideIdColumn(DataGridView dgv)
        {
            if (dgv.Columns.Contains("Id")) dgv.Columns["Id"]!.Visible = false;
        }

        private static void SetColumnWeights(DataGridView dgv, Dictionary<string, float> weights)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
                if (weights.TryGetValue(col.Name, out float w)) col.FillWeight = w;
        }

        // ── 雙擊貼文 → PostViewForm ───────────────────────────

        private void dgvWithLineId_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
            => OpenPost(dgvWithLineId, e.RowIndex);

        private void dgvWithoutLineId_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
            => OpenPost(dgvWithoutLineId, e.RowIndex);

        private void OpenPost(DataGridView dgv, int rowIndex)
        {
            if (rowIndex < 0) return;
            var id = dgv.Rows[rowIndex].Cells["Id"].Value?.ToString();
            if (string.IsNullOrEmpty(id)) return;
            var post = _db.GetPost(id);
            if (post == null) return;
            using var f = new PostViewForm(post, _db);
            f.ShowDialog(this);
        }

        // ── 狀態切換 ──────────────────────────────────────────

        private void SetRunningState(bool running)
        {
            if (InvokeRequired) { Invoke(() => SetRunningState(running)); return; }
            _isRunning               = running;
            btnStartAnalyze.Text     = running ? "取消" : "開始分析";
            rbScopePage.Enabled      = !running;
            rbScopeAll.Enabled       = !running;
            btnExport.Enabled        = !running && _withLineId.Count > 0;
            pnlProgress.Visible      = running || progressBar.Value > 0;
        }

        // ── 匯出 CSV ──────────────────────────────────────────

        private void btnExport_Click(object sender, EventArgs e)
        {
            // 收集所有待匯出項目，先以 Line ID 去重（相同 Line ID 只保留最新一筆貼文）
            var allItems = _withLineId
                .SelectMany(x => x.LineIds.Select(id => (Post: x.Post, LineId: id)))
                .GroupBy(x => x.LineId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            // 已匯出的 Line ID（忽略大小寫）
            var exportedLineIds = _db.GetExportedLineIds();

            int newCount   = allItems.Count(x => !exportedLineIds.Contains(x.LineId));
            int totalCount = allItems.Count;

            using var dlg = new ExportLineIdForm(newCount, totalCount);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var toExport = dlg.ExportNewOnly
                ? allItems.Where(x => !exportedLineIds.Contains(x.LineId)).ToList()
                : allItems;

            if (toExport.Count == 0)
            {
                MessageBox.Show("沒有符合條件的 Line ID 可以匯出。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                WriteCsv(dlg.FilePath, toExport);
                _db.MarkLineIdsAsExported(toExport.Select(x => (x.Post.Id, x.LineId)));
                AppLogger.Info($"已匯出 {toExport.Count} 筆 Line ID 至：{dlg.FilePath}");
                MessageBox.Show($"成功匯出 {toExport.Count} 筆 Line ID。\n\n{dlg.FilePath}",
                    "匯出完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"匯出 CSV 失敗：{ex.Message}", ex);
                MessageBox.Show($"匯出失敗：{ex.Message}",
                    "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void WriteCsv(string path, List<(Post Post, string LineId)> items)
        {
            var sb = new StringBuilder();
            // BOM，讓 Excel 正確開啟 UTF-8
            sb.Append('\uFEFF');
            sb.AppendLine("看版,貼文ID,標題,作者,作者暱稱,貼文時間,Line ID");
            foreach (var (post, lineId) in items)
            {
                sb.AppendLine(string.Join(",",
                    CsvField(post.Board),
                    CsvField(post.Id),
                    CsvField(post.Title),
                    CsvField(post.AuthorId),
                    CsvField(post.AuthorNick),
                    CsvField(post.PostTime),
                    CsvField(lineId)));
            }
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }

        private static string CsvField(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
