using LineIDAnalyzer.Models;
using System.Text;

namespace LineIDAnalyzer.Forms
{
    public partial class TestResultForm : Form
    {
        private readonly TestRunSummary _summary;

        public TestResultForm(TestRunSummary summary)
        {
            _summary = summary;
            InitializeComponent();
            PopulateResults();
        }

        private void PopulateResults()
        {
            // ── 彙總標題 ─────────────────────────────────────
            var elapsed = _summary.TotalDuration.TotalSeconds;
            lblSummary.Text =
                $"共 {_summary.Total} 個測試　　" +
                $"✅ 通過 {_summary.Passed}　　" +
                $"❌ 失敗 {_summary.Failed}　　" +
                $"耗時 {elapsed:F1} 秒";

            lblSummary.ForeColor = _summary.Failed == 0 ? Color.DarkGreen : Color.Red;

            if (_summary.Stage1FalseNegatives > 0)
            {
                lblStage1Warning.Text    = $"⚠ 其中 {_summary.Stage1FalseNegatives} 個為 Stage 1 誤判（關鍵字未命中）";
                lblStage1Warning.Visible = true;
            }

            // ── 填入 DataGridView ─────────────────────────────
            foreach (var r in _summary.Results)
            {
                var stage1Text = r.Stage1Result == Stage1Result.KeywordFound ? "命中" : "未命中";
                var ids        = string.Join(", ", r.ExtractedIds);
                var resultText = r.Passed ? "✅ 通過" : "❌ 失敗";
                var reason     = r.FailureReason ?? string.Empty;

                var rowIdx = dgvResults.Rows.Add(
                    r.TestCase.FileName,
                    r.TestCase.ExpectsLineId ? "有 ID" : "無 ID",
                    stage1Text,
                    string.IsNullOrEmpty(ids) ? "—" : ids,
                    $"{r.Duration.TotalSeconds:F1}s",
                    resultText,
                    reason);

                var row = dgvResults.Rows[rowIdx];
                row.DefaultCellStyle.BackColor = r.Passed ? Color.Honeydew : Color.MistyRose;

                if (!r.Passed && r.Stage1Result == Stage1Result.KeywordNotFound && r.TestCase.ExpectsLineId)
                    row.DefaultCellStyle.BackColor = Color.LightYellow; // Stage 1 誤判用黃色區分
            }
        }

        // ── 匯出 CSV ──────────────────────────────────────────
        private void btnExport_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter      = "CSV 檔案 (*.csv)|*.csv|文字檔 (*.txt)|*.txt",
                FileName    = $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt  = "csv"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var sb = new StringBuilder();
            sb.AppendLine("檔案名稱,預期,Stage1,萃取到的ID,耗時,結果,失敗原因");

            foreach (var r in _summary.Results)
            {
                var ids    = string.Join(" / ", r.ExtractedIds);
                var reason = (r.FailureReason ?? string.Empty).Replace(",", "，");
                sb.AppendLine(
                    $"\"{r.TestCase.FileName}\"," +
                    $"{(r.TestCase.ExpectsLineId ? "有ID" : "無ID")}," +
                    $"{(r.Stage1Result == Stage1Result.KeywordFound ? "命中" : "未命中")}," +
                    $"\"{ids}\"," +
                    $"{r.Duration.TotalSeconds:F1}s," +
                    $"{(r.Passed ? "通過" : "失敗")}," +
                    $"\"{reason}\"");
            }

            sb.AppendLine();
            sb.AppendLine($"彙總,共 {_summary.Total} 個,通過 {_summary.Passed},失敗 {_summary.Failed},耗時 {_summary.TotalDuration.TotalSeconds:F1}s,,");

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"已匯出至：\n{dlg.FileName}", "匯出成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
