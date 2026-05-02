namespace LineIDAnalyzer.Forms
{
    partial class TestResultForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblSummary       = new Label();
            lblStage1Warning = new Label();
            dgvResults       = new DataGridView();
            pnlBottom        = new Panel();
            btnExport        = new Button();
            btnClose         = new Button();

            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();

            // ── lblSummary ────────────────────────────────────
            lblSummary.Dock      = DockStyle.Top;
            lblSummary.Height    = 30;
            lblSummary.Font      = new Font(Font.FontFamily, 11f, FontStyle.Bold);
            lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            lblSummary.Padding   = new Padding(8, 0, 0, 0);

            // ── lblStage1Warning ──────────────────────────────
            lblStage1Warning.Dock      = DockStyle.Top;
            lblStage1Warning.Height    = 22;
            lblStage1Warning.ForeColor = Color.DarkOrange;
            lblStage1Warning.Font      = new Font(Font.FontFamily, 9f);
            lblStage1Warning.TextAlign = ContentAlignment.MiddleLeft;
            lblStage1Warning.Padding   = new Padding(8, 0, 0, 0);
            lblStage1Warning.Visible   = false;

            // ── dgvResults ────────────────────────────────────
            dgvResults.Dock                  = DockStyle.Fill;
            dgvResults.ReadOnly              = true;
            dgvResults.AllowUserToAddRows    = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.AllowUserToResizeRows = false;
            dgvResults.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResults.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.RowHeadersVisible     = false;
            dgvResults.BackgroundColor       = Color.White;
            dgvResults.BorderStyle           = BorderStyle.None;

            // 欄位定義
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFile", HeaderText = "檔案名稱", FillWeight = 18
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colExpect", HeaderText = "預期", FillWeight = 7
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStage1", HeaderText = "Stage 1", FillWeight = 8
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIds", HeaderText = "萃取到的 Line ID", FillWeight = 20
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDuration", HeaderText = "耗時", FillWeight = 6
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colResult", HeaderText = "結果", FillWeight = 8
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReason", HeaderText = "失敗原因", FillWeight = 33
            });

            // ── pnlBottom ─────────────────────────────────────
            pnlBottom.Dock    = DockStyle.Bottom;
            pnlBottom.Height  = 44;
            pnlBottom.Padding = new Padding(8, 7, 8, 7);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Controls.Add(btnExport);

            btnClose.Text     = "關閉";
            btnClose.Width    = 80;
            btnClose.Height   = 30;
            btnClose.Dock     = DockStyle.Right;
            btnClose.Click   += btnClose_Click;

            btnExport.Text    = "匯出 CSV";
            btnExport.Width   = 100;
            btnExport.Height  = 30;
            btnExport.Dock    = DockStyle.Right;
            btnExport.Margin  = new Padding(0, 0, 4, 0);
            btnExport.Click  += btnExport_Click;

            // ── TestResultForm ────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(980, 560);
            MinimumSize         = new Size(700, 400);
            StartPosition       = FormStartPosition.CenterParent;
            Text                = "測試結果";

            Controls.Add(dgvResults);
            Controls.Add(lblStage1Warning);
            Controls.Add(lblSummary);
            Controls.Add(pnlBottom);

            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Label             lblSummary;
        private Label             lblStage1Warning;
        private DataGridView      dgvResults;
        private Panel             pnlBottom;
        private Button            btnExport;
        private Button            btnClose;
    }
}
