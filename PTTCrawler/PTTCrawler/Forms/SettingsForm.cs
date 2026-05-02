using PTTCrawler.Data;
using PTTCrawler.Logging;
using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly DatabaseManager _db;

        public SettingsForm(DatabaseManager db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadSettings();
        }

        private void LoadSettings()
        {
            var s = _db.LoadAppSettings();
            tbApiKey.Text        = s.ApiKey;
            tbModelName.Text     = s.ModelName;
            nudTimeout.Value     = Math.Clamp(s.TimeoutSeconds, (int)nudTimeout.Minimum, (int)nudTimeout.Maximum);
        }

        private void btnToggleVisible_Click(object sender, EventArgs e)
        {
            tbApiKey.UseSystemPasswordChar = !tbApiKey.UseSystemPasswordChar;
            btnToggleVisible.Text = tbApiKey.UseSystemPasswordChar ? "顯示" : "隱藏";
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            var apiKey = tbApiKey.Text.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("請先輸入 API Key。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnTest.Enabled = btnSave.Enabled = false;
            lblTestResult.Text = "連線測試中…";

            try
            {
                var settings = new AppSettings { ApiKey = apiKey, ModelName = tbModelName.Text.Trim() };
                var analyzer = new Business.LineIdAnalyzer(settings);
                var (success, message) = await analyzer.TestConnectionAsync();
                lblTestResult.Text      = message;
                lblTestResult.ForeColor = success ? Color.Green : Color.Red;
            }
            catch (Exception ex)
            {
                lblTestResult.Text      = $"錯誤：{ex.Message}";
                lblTestResult.ForeColor = Color.Red;
            }
            finally
            {
                btnTest.Enabled = btnSave.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var apiKey = tbApiKey.Text.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("API Key 不可為空白。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var settings = new AppSettings
            {
                ApiKey         = apiKey,
                ModelName      = tbModelName.Text.Trim(),
                TimeoutSeconds = (int)nudTimeout.Value
            };

            _db.SaveAppSettings(settings);
            AppLogger.Info("設定已儲存。");
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
