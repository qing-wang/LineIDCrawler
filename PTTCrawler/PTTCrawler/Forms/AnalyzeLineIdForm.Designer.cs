namespace PTTCrawler.Forms
{
    partial class AnalyzeLineIdForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlTop           = new Panel();
            lblScope         = new Label();
            rbScopePage      = new RadioButton();
            rbScopeAll       = new RadioButton();
            btnStartAnalyze  = new Button();
            pnlProgress      = new Panel();
            progressBar      = new ProgressBar();
            lblProgress      = new Label();
            grpWithLineId    = new GroupBox();
            dgvWithLineId    = new DataGridView();
            grpWithoutLineId = new GroupBox();
            dgvWithoutLineId = new DataGridView();

            pnlTop.SuspendLayout();
            pnlProgress.SuspendLayout();
            grpWithLineId.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWithLineId).BeginInit();
            grpWithoutLineId.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWithoutLineId).BeginInit();
            SuspendLayout();

            // ── pnlTop ────────────────────────────────────────
            pnlTop.Dock    = DockStyle.Top;
            pnlTop.Height  = 44;
            pnlTop.Padding = new Padding(8, 6, 8, 4);
            pnlTop.Controls.AddRange(new Control[] { lblScope, rbScopePage, rbScopeAll, btnStartAnalyze });

            lblScope.Text     = "分析範圍：";
            lblScope.Location = new Point(8, 12);
            lblScope.AutoSize = true;

            rbScopePage.Text     = "目前這一頁";
            rbScopePage.Location = new Point(80, 10);
            rbScopePage.AutoSize = true;
            rbScopePage.Checked  = true;

            rbScopeAll.Text     = "所有貼文";
            rbScopeAll.Location = new Point(186, 10);
            rbScopeAll.AutoSize = true;

            btnStartAnalyze.Text     = "開始分析";
            btnStartAnalyze.Size     = new Size(90, 28);
            btnStartAnalyze.Location = new Point(290, 7);
            btnStartAnalyze.Click   += btnStartAnalyze_Click;

            // ── pnlProgress ───────────────────────────────────
            pnlProgress.Dock    = DockStyle.Top;
            pnlProgress.Height  = 52;
            pnlProgress.Padding = new Padding(8, 4, 8, 4);
            pnlProgress.Visible = false;
            pnlProgress.Controls.AddRange(new Control[] { progressBar, lblProgress });

            progressBar.Location = new Point(8, 6);
            progressBar.Size     = new Size(860, 18);
            progressBar.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            lblProgress.Text     = string.Empty;
            lblProgress.Location = new Point(8, 28);
            lblProgress.Size     = new Size(860, 18);
            lblProgress.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblProgress.AutoSize = false;

            // ── grpWithLineId ─────────────────────────────────
            grpWithLineId.Text     = "含 Line ID 的貼文";
            grpWithLineId.Dock     = DockStyle.Top;
            grpWithLineId.Height   = 220;
            grpWithLineId.Padding  = new Padding(4);
            grpWithLineId.Controls.Add(dgvWithLineId);

            dgvWithLineId.Dock                  = DockStyle.Fill;
            dgvWithLineId.ReadOnly              = true;
            dgvWithLineId.AllowUserToAddRows    = false;
            dgvWithLineId.AllowUserToDeleteRows = false;
            dgvWithLineId.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgvWithLineId.MultiSelect           = false;
            dgvWithLineId.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            dgvWithLineId.RowHeadersVisible     = false;
            dgvWithLineId.CellDoubleClick      += dgvWithLineId_CellDoubleClick;

            // ── grpWithoutLineId ──────────────────────────────
            grpWithoutLineId.Text    = "不含 Line ID 的貼文";
            grpWithoutLineId.Dock    = DockStyle.Fill;
            grpWithoutLineId.Padding = new Padding(4);
            grpWithoutLineId.Controls.Add(dgvWithoutLineId);

            dgvWithoutLineId.Dock                  = DockStyle.Fill;
            dgvWithoutLineId.ReadOnly              = true;
            dgvWithoutLineId.AllowUserToAddRows    = false;
            dgvWithoutLineId.AllowUserToDeleteRows = false;
            dgvWithoutLineId.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgvWithoutLineId.MultiSelect           = false;
            dgvWithoutLineId.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            dgvWithoutLineId.RowHeadersVisible     = false;
            dgvWithoutLineId.CellDoubleClick      += dgvWithoutLineId_CellDoubleClick;

            // ── AnalyzeLineIdForm ─────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(1000, 680);
            MinimumSize         = new Size(700, 500);
            Text                = "分析 Line ID";
            StartPosition       = FormStartPosition.CenterParent;

            Controls.Add(grpWithoutLineId);
            Controls.Add(grpWithLineId);
            Controls.Add(pnlProgress);
            Controls.Add(pnlTop);

            pnlTop.ResumeLayout(false);
            pnlProgress.ResumeLayout(false);
            grpWithLineId.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvWithLineId).EndInit();
            grpWithoutLineId.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvWithoutLineId).EndInit();
            ResumeLayout(false);
        }

        private Panel           pnlTop;
        private Label           lblScope;
        private RadioButton     rbScopePage;
        private RadioButton     rbScopeAll;
        private Button          btnStartAnalyze;
        private Panel           pnlProgress;
        private ProgressBar     progressBar;
        private Label           lblProgress;
        private GroupBox        grpWithLineId;
        private DataGridView    dgvWithLineId;
        private GroupBox        grpWithoutLineId;
        private DataGridView    dgvWithoutLineId;
    }
}
