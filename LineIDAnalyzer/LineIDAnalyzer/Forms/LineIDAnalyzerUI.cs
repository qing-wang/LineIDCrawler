using LineIDAnalyzer.Business;
using LineIDAnalyzer.Data;
using LineIDAnalyzer.Forms;
using LineIDAnalyzer.Logging;
using LineIDAnalyzer.Models;

namespace LineIDAnalyzer.Forms
{
    public partial class LineIDAnalyzerUI : Form
    {
        private readonly DatabaseManager         _db;
        private CancellationTokenSource?         _analysisCts;

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
            AppLogger.Info("使用者取消分析。");
        }

        // ── 輔助方法 ──────────────────────────────────────────

        private void SetAnalyzingState(bool isAnalyzing)
        {
            btnAnalyze.Enabled       = !isAnalyzing;
            btnCancelAnalysis.Enabled = isAnalyzing;

            if (isAnalyzing)
                SetStatusText("分析中，請稍候…", Color.DarkBlue);
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
            _db.Dispose();
            base.OnFormClosing(e);
        }
    }
}
