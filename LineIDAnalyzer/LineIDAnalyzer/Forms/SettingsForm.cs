using LineIDAnalyzer.Business;
using LineIDAnalyzer.Data;
using LineIDAnalyzer.Logging;
using LineIDAnalyzer.Models;

namespace LineIDAnalyzer.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly DatabaseManager _db;

        public SettingsForm(DatabaseManager db)
        {
            _db = db;
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var apiKey = _db.LoadApiKey();
            if (!string.IsNullOrEmpty(apiKey))
                tbApiKey.Text = apiKey;

            var model = _db.GetSetting("model_name");
            if (!string.IsNullOrEmpty(model))
                tbModelName.Text = model;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbApiKey.Text))
            {
                MessageBox.Show("請輸入 API Key。", "驗證失敗",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _db.SaveApiKey(tbApiKey.Text.Trim());
            _db.SaveSetting("model_name", tbModelName.Text.Trim());

            AppLogger.Info("設定已儲存。");
            MessageBox.Show("設定已儲存。", "儲存成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private async void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbApiKey.Text))
            {
                MessageBox.Show("請先輸入 API Key 再測試。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnTestConnection.Enabled = false;
            btnTestConnection.Text    = "測試中…";
            AppLogger.Info("開始測試 API Key 連線…");

            try
            {
                var settings = new AppSettings
                {
                    ApiKey    = tbApiKey.Text.Trim(),
                    ModelName = string.IsNullOrWhiteSpace(tbModelName.Text)
                                ? "gpt-4o-mini"
                                : tbModelName.Text.Trim()
                };

                var analyzer            = new LineIDAnalyzer.Business.LineIDAnalyzer(settings);
                var (success, message) = await analyzer.TestConnectionAsync();

                if (success)
                {
                    AppLogger.Info($"連線測試結果：{message}");
                    MessageBox.Show(message, "連線成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppLogger.Error($"連線測試失敗：{message}");
                    MessageBox.Show(message, "連線失敗",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("連線測試時發生例外", ex);
                MessageBox.Show($"發生錯誤：{ex.Message}", "錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestConnection.Enabled = true;
                btnTestConnection.Text    = "測試連線";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnToggleApiKeyVisibility_Click(object sender, EventArgs e)
        {
            tbApiKey.UseSystemPasswordChar = !tbApiKey.UseSystemPasswordChar;
            btnToggleApiKeyVisibility.Text = tbApiKey.UseSystemPasswordChar ? "顯示" : "隱藏";
        }
    }
}
