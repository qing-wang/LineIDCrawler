namespace PTTCrawler.Forms
{
    /// <summary>讓使用者選擇 Line ID CSV 匯出範圍及目的檔案路徑。</summary>
    public partial class ExportLineIdForm : Form
    {
        public bool   ExportNewOnly { get; private set; } = true;
        public string FilePath      { get; private set; } = string.Empty;

        private readonly int _newCount;
        private readonly int _totalCount;

        public ExportLineIdForm(int newCount, int totalCount)
        {
            InitializeComponent();
            _newCount   = newCount;
            _totalCount = totalCount;
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            rbNewOnly.Text = $"只匯出未匯出過的 Line ID（{_newCount} 筆）";
            rbAll.Text     = $"全部匯出（{_totalCount} 筆）";
        }

        private void rbNewOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (rbNewOnly.Checked) ExportNewOnly = true;
            UpdateOkButton();
        }

        private void rbAll_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAll.Checked) ExportNewOnly = false;
            UpdateOkButton();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title            = "選擇匯出檔案",
                Filter           = "CSV 檔案 (*.csv)|*.csv|所有檔案 (*.*)|*.*",
                DefaultExt       = "csv",
                FileName         = $"LineID_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                OverwritePrompt  = true
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtFilePath.Text = dlg.FileName;
                UpdateOkButton();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            FilePath     = txtFilePath.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void UpdateOkButton()
        {
            btnOk.Enabled = !string.IsNullOrWhiteSpace(txtFilePath.Text);
        }

        private void txtFilePath_TextChanged(object sender, EventArgs e) => UpdateOkButton();
    }
}
