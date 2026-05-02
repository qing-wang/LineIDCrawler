using LineIDAnalyzer.Business;
using LineIDAnalyzer.Data;
using LineIDAnalyzer.Forms;
using LineIDAnalyzer.Logging;
using LineIDAnalyzer.Models;

namespace LineIDAnalyzer.Forms
{
    public partial class LineIDAnalyzerUI : Form
    {
        private readonly DatabaseManager _db;
        private CancellationTokenSource? _analysisCts;
        private CancellationTokenSource? _testRunCts;
        private CancellationTokenSource? _profileAnalysisCts;

        public LineIDAnalyzerUI()
        {
            InitializeComponent();

            // 初始化資料庫（放置在執行目錄下）
            var dbPath = Path.Combine(AppContext.BaseDirectory, "lineIdAnalyzer.db");
            _db = new DatabaseManager(dbPath);

            // 繫結 AppLogger 到 Console Log RichTextBox
            AppLogger.ConsoleLogBox = tbConsoleLog;

            AppLogger.Info("應用程式啟動。");
        }

        // ── 按鈕事件 ──────────────────────────────────────────

        private async void btnAnalyze_Click(object sender, EventArgs e)
        {
            var inputText = tbInputText.Text.Trim();

            if (string.IsNullOrWhiteSpace(inputText))
            {
                AppLogger.Info("使用者未輸入任何文字，分析已中止。");
                MessageBox.Show("請先輸入待分析的文字。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetAnalyzingState(true);
            AppLogger.Info("開始分析輸入文字…");

            try
            {
                var apiKey = _db.LoadApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    AppLogger.Error("尚未設定 API Key，請先至「設定」頁面輸入。");
                    MessageBox.Show("尚未設定 API Key，請先至「設定」頁面輸入。", "缺少設定",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Stage 1 結果先記錄，供使用者在 Console Log 觀察
                var hasKeyword = LineIDAnalyzer.Business.LineIDAnalyzer.ContainsLineKeyword(inputText);
                if (hasKeyword)
                    AppLogger.Debug("Stage 1：偵測到 LINE 相關關鍵字，進入 LLM 分析。");
                else
                    AppLogger.Debug("Stage 1：未偵測到 LINE 相關關鍵字，略過 LLM 分析。");

                var modelName = _db.GetSetting("model_name") ?? "gpt-4o-mini";
                var settings  = new AppSettings { ApiKey = apiKey, ModelName = modelName };
                var analyzer  = new LineIDAnalyzer.Business.LineIDAnalyzer(settings);

                _analysisCts  = new CancellationTokenSource();
                var result    = await analyzer.AnalyzeAsync(inputText, _analysisCts.Token);

                if (!result.IsSuccess)
                {
                    AppLogger.Error($"分析失敗：{result.ErrorMessage}");
                    SetStatusText($"分析失敗：{result.ErrorMessage}", Color.Red);
                    tbAnalysisResult.Text = string.Empty;
                    return;
                }

                _db.SaveAnalysisHistory(inputText, result);

                if (result.HasLineId)
                {
                    var ids = string.Join(", ", result.ExtractedIds);
                    tbAnalysisResult.Text = ids;
                    AppLogger.Info($"偵測到 {result.ExtractedIds.Count} 個 Line ID：{ids}");
                    SetStatusText($"偵測到 {result.ExtractedIds.Count} 個 Line ID。", Color.DarkGreen);
                }
                else
                {
                    tbAnalysisResult.Text = string.Empty;
                    AppLogger.Info("未偵測到任何 Line ID。");
                    SetStatusText("未偵測到任何 Line ID。", SystemColors.ControlText);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("分析過程中發生未預期的例外", ex);
                SetStatusText("分析時發生錯誤，請查看 Console Log。", Color.Red);
            }
            finally
            {
                _analysisCts?.Dispose();
                _analysisCts = null;
                SetAnalyzingState(false);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            AppLogger.Debug("開啟設定視窗。");
            using var form = new SettingsForm(_db);
            form.ShowDialog(this);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            tbInputText.Text      = string.Empty;
            tbAnalysisResult.Text = string.Empty;
            AppLogger.Debug("已清除輸入文字與分析結果。");
            SetStatusText("就緒", SystemColors.ControlText);
        }

        private void btnCopyResult_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbAnalysisResult.Text))
            {
                AppLogger.Debug("目前沒有可複製的分析結果。");
                return;
            }
            Clipboard.SetText(tbAnalysisResult.Text);
            AppLogger.Info("分析結果已複製到剪貼簿。");
            SetStatusText("已複製到剪貼簿。", SystemColors.ControlText);
        }

        private void btnCancelAnalysis_Click(object sender, EventArgs e)
        {
            _analysisCts?.Cancel();
            _testRunCts?.Cancel();
            _profileAnalysisCts?.Cancel();
            AppLogger.Info("使用者取消操作。");
        }

        private async void btnProfileAnalyze_Click(object sender, EventArgs e)
        {
            var body = tbInputText.Text.Trim();
            if (string.IsNullOrWhiteSpace(body))
            {
                AppLogger.Info("使用者未輸入任何文字，人物分析已中止。");
                MessageBox.Show("請先輸入待分析的文字（內文）。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetProfileAnalyzingState(true);
            AppLogger.Info("開始執行人物分析…");

            try
            {
                var apiKey = _db.LoadApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    AppLogger.Error("尚未設定 API Key，請先至「設定」頁面輸入。");
                    MessageBox.Show("尚未設定 API Key，請先至「設定」頁面輸入。", "缺少設定",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var modelName = _db.GetSetting("model_name") ?? "gpt-4o-mini";
                var settings  = new AppSettings { ApiKey = apiKey, ModelName = modelName };
                var analyzer  = new LineIDAnalyzer.Business.AuthorProfileAnalyzer(settings);

                var request = new AuthorProfileRequest
                {
                    Title    = string.IsNullOrWhiteSpace(tbTitle.Text.Trim())    ? null : tbTitle.Text.Trim(),
                    AuthorId = string.IsNullOrWhiteSpace(tbAuthorId.Text.Trim()) ? null : tbAuthorId.Text.Trim(),
                    Nickname = string.IsNullOrWhiteSpace(tbNickname.Text.Trim()) ? null : tbNickname.Text.Trim(),
                    Body     = body
                };

                _profileAnalysisCts = new CancellationTokenSource();
                var profile = await analyzer.AnalyzeAsync(request, _profileAnalysisCts.Token);

                if (!profile.IsSuccess)
                {
                    AppLogger.Error($"人物分析失敗：{profile.ErrorMessage}");
                    SetStatusText($"人物分析失敗：{profile.ErrorMessage}", Color.Red);
                    return;
                }

                _db.SaveProfileHistory(request, profile);
                AppLogger.Info("人物分析完成，開啟結果視窗。");
                SetStatusText("人物分析完成。", Color.DarkGreen);

                using var form = new AuthorProfileResultForm(profile);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                AppLogger.Error("人物分析過程中發生未預期的例外", ex);
                SetStatusText("人物分析時發生錯誤，請查看系統日誌。", Color.Red);
            }
            finally
            {
                _profileAnalysisCts?.Dispose();
                _profileAnalysisCts = null;
                SetProfileAnalyzingState(false);
            }
        }

        private void SetProfileAnalyzingState(bool isAnalyzing)
        {
            btnProfileAnalyze.Enabled = !isAnalyzing;
            btnAnalyze.Enabled        = !isAnalyzing;
            btnRunTests.Enabled       = !isAnalyzing;
            btnCancelAnalysis.Enabled = isAnalyzing;

            if (isAnalyzing)
                SetStatusText("人物分析中，請稍候…", Color.DarkBlue);
        }

        private async void btnRunTests_Click(object sender, EventArgs e)
        {
            var apiKey = _db.LoadApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                AppLogger.Error("尚未設定 API Key，無法執行測試。");
                MessageBox.Show("尚未設定 API Key，請先至「設定」頁面輸入。", "缺少設定",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 計算測試案例數量，若無則提示
            var testCasesRoot = Path.Combine(AppContext.BaseDirectory, "testcases");
            var lineIdDir   = Path.Combine(testCasesRoot, "lineid");
            var noLineIdDir = Path.Combine(testCasesRoot, "nolineid");
            var totalCount  = (Directory.Exists(lineIdDir)   ? Directory.GetFiles(lineIdDir,   "*.txt").Length : 0)
                            + (Directory.Exists(noLineIdDir) ? Directory.GetFiles(noLineIdDir, "*.txt").Length : 0);

            if (totalCount == 0)
            {
                AppLogger.Info($"測試目錄中尚無任何 .txt 測試案例（路徑：{testCasesRoot}）。");
                MessageBox.Show(
                    $"測試目錄中尚無任何 .txt 檔案。\n\n請將測試案例放入：\n  {lineIdDir}\n  {noLineIdDir}",
                    "沒有測試案例", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetTestRunningState(true);
            AppLogger.Info($"開始執行測試，共 {totalCount} 個測試案例…");

            try
            {
                var modelName = _db.GetSetting("model_name") ?? "gpt-4o-mini";
                var settings  = new AppSettings { ApiKey = apiKey, ModelName = modelName };
                var runner    = new TestRunner(settings, testCasesRoot);

                _testRunCts = new CancellationTokenSource();

                var progress = new Progress<TestProgress>(p =>
                {
                    SetStatusText($"測試中：{p.Current} / {p.Total}　{p.CurrentFileName}", Color.DarkBlue);
                    AppLogger.Debug($"[{p.Current}/{p.Total}] 正在分析：{p.CurrentFileName}");
                });

                var summary = await runner.RunAllAsync(progress, _testRunCts.Token);

                AppLogger.Info($"測試完成：共 {summary.Total} 個，通過 {summary.Passed}，失敗 {summary.Failed}，耗時 {summary.TotalDuration.TotalSeconds:F1} 秒。");

                if (summary.Failed == 0)
                    SetStatusText($"✅ 全部 {summary.Total} 個測試通過！", Color.DarkGreen);
                else
                    SetStatusText($"❌ {summary.Failed} 個測試失敗（共 {summary.Total} 個）", Color.Red);

                using var resultForm = new TestResultForm(summary);
                resultForm.ShowDialog(this);
            }
            catch (OperationCanceledException)
            {
                AppLogger.Info("測試執行已被使用者取消。");
                SetStatusText("測試已取消。", SystemColors.ControlText);
            }
            catch (Exception ex)
            {
                AppLogger.Error("執行測試時發生未預期的例外", ex);
                SetStatusText("測試執行時發生錯誤，請查看 Console Log。", Color.Red);
            }
            finally
            {
                _testRunCts?.Dispose();
                _testRunCts = null;
                SetTestRunningState(false);
            }
        }

        // ── 輔助方法 ──────────────────────────────────────────

        private void SetAnalyzingState(bool isAnalyzing)
        {
            btnAnalyze.Enabled        = !isAnalyzing;
            btnRunTests.Enabled       = !isAnalyzing;
            btnCancelAnalysis.Enabled = isAnalyzing;

            if (isAnalyzing)
                SetStatusText("分析中，請稍候…", Color.DarkBlue);
        }

        private void SetTestRunningState(bool isRunning)
        {
            btnAnalyze.Enabled        = !isRunning;
            btnRunTests.Enabled       = !isRunning;
            btnCancelAnalysis.Enabled = isRunning;
        }

        private void SetStatusText(string text, Color color)
        {
            statusLabel.Text      = text;
            statusLabel.ForeColor = color;
        }

        // ── 表單關閉 ──────────────────────────────────────────

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AppLogger.Info("應用程式關閉。");
            _analysisCts?.Cancel();
            _testRunCts?.Cancel();
            _profileAnalysisCts?.Cancel();
            _db.Dispose();
            base.OnFormClosing(e);
        }
    }
}
