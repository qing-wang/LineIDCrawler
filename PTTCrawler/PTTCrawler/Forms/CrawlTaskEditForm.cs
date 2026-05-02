using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class CrawlTaskEditForm : Form
    {
        public CrawlTask Result { get; private set; } = new();
        private readonly bool _isNew;

        public CrawlTaskEditForm(CrawlTask? existing = null)
        {
            InitializeComponent();
            _isNew = (existing == null);
            Text   = _isNew ? "新增爬蟲任務" : "修改爬蟲任務";

            if (!_isNew && existing != null)
                LoadTask(existing);
        }

        private void LoadTask(CrawlTask t)
        {
            tbName.Text      = t.Name;
            cbTaskType.Text  = TaskTypeToDisplay(t.TaskType);
            tbBoardUrl.Text  = t.BoardUrl;
            tbKeyword.Text   = t.Keyword ?? string.Empty;
            if (t.MaxPages.HasValue)
            {
                cbUnlimited.Checked  = false;
                nudMaxPages.Value    = t.MaxPages.Value;
            }
            else
            {
                cbUnlimited.Checked = true;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("請輸入任務名稱。", "驗證失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(tbBoardUrl.Text))
            {
                MessageBox.Show("請輸入看版網址。", "驗證失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbBoardUrl.Focus();
                return;
            }

            Result.Name      = tbName.Text.Trim();
            Result.TaskType  = CrawlTaskType.CollectLineId;
            Result.BoardUrl  = tbBoardUrl.Text.Trim();
            Result.Keyword   = string.IsNullOrWhiteSpace(tbKeyword.Text) ? null : tbKeyword.Text.Trim();
            Result.MaxPages  = cbUnlimited.Checked ? null : (int)nudMaxPages.Value;
            Result.Status    = "Active";

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cbUnlimited_CheckedChanged(object sender, EventArgs e)
        {
            nudMaxPages.Enabled = !cbUnlimited.Checked;
        }

        private static string TaskTypeToDisplay(CrawlTaskType type) => type switch
        {
            CrawlTaskType.CollectLineId => "收集 Line ID",
            _                           => type.ToString()
        };
    }
}
