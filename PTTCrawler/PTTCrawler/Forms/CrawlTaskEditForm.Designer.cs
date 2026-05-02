namespace PTTCrawler.Forms
{
    partial class CrawlTaskEditForm
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
            lblName      = new Label();
            tbName       = new TextBox();
            lblTaskType  = new Label();
            cbTaskType   = new ComboBox();
            lblBoardUrl  = new Label();
            tbBoardUrl   = new TextBox();
            lblKeyword   = new Label();
            tbKeyword    = new TextBox();
            lblMaxPages  = new Label();
            nudMaxPages  = new NumericUpDown();
            cbUnlimited  = new CheckBox();
            btnOk        = new Button();
            btnCancel    = new Button();

            ((System.ComponentModel.ISupportInitialize)nudMaxPages).BeginInit();
            SuspendLayout();

            // ── lblName ───────────────────────────────────────
            lblName.Text     = "任務名稱：";
            lblName.Location = new Point(12, 16);
            lblName.AutoSize = true;

            // ── tbName ────────────────────────────────────────
            tbName.Location = new Point(110, 13);
            tbName.Size     = new Size(300, 23);

            // ── lblTaskType ───────────────────────────────────
            lblTaskType.Text     = "任務性質：";
            lblTaskType.Location = new Point(12, 50);
            lblTaskType.AutoSize = true;

            // ── cbTaskType ────────────────────────────────────
            cbTaskType.Location      = new Point(110, 47);
            cbTaskType.Size          = new Size(180, 23);
            cbTaskType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTaskType.Items.Add("收集 Line ID");
            cbTaskType.SelectedIndex = 0;

            // ── lblBoardUrl ───────────────────────────────────
            lblBoardUrl.Text     = "看版網址：";
            lblBoardUrl.Location = new Point(12, 84);
            lblBoardUrl.AutoSize = true;

            // ── tbBoardUrl ────────────────────────────────────
            tbBoardUrl.Location = new Point(110, 81);
            tbBoardUrl.Size     = new Size(350, 23);

            // ── lblKeyword ────────────────────────────────────
            lblKeyword.Text     = "爬取關鍵字：";
            lblKeyword.Location = new Point(12, 118);
            lblKeyword.AutoSize = true;

            // ── tbKeyword ─────────────────────────────────────
            tbKeyword.Location    = new Point(110, 115);
            tbKeyword.Size        = new Size(300, 23);
            tbKeyword.PlaceholderText = "留空表示全爬";

            // ── lblMaxPages ───────────────────────────────────
            lblMaxPages.Text     = "爬取頁數上限：";
            lblMaxPages.Location = new Point(12, 152);
            lblMaxPages.AutoSize = true;

            // ── nudMaxPages ───────────────────────────────────
            nudMaxPages.Location = new Point(110, 149);
            nudMaxPages.Size     = new Size(80, 23);
            nudMaxPages.Minimum  = 1;
            nudMaxPages.Maximum  = 9999;
            nudMaxPages.Value    = 10;

            // ── cbUnlimited ───────────────────────────────────
            cbUnlimited.Text     = "不限";
            cbUnlimited.Location = new Point(200, 150);
            cbUnlimited.AutoSize = true;
            cbUnlimited.Checked  = true;
            nudMaxPages.Enabled  = false;
            cbUnlimited.CheckedChanged += cbUnlimited_CheckedChanged;

            // ── btnOk ─────────────────────────────────────────
            btnOk.Text         = "確定";
            btnOk.Size         = new Size(90, 30);
            btnOk.Location     = new Point(220, 195);
            btnOk.DialogResult = DialogResult.None;
            btnOk.Click       += btnOk_Click;

            // ── btnCancel ─────────────────────────────────────
            btnCancel.Text         = "取消";
            btnCancel.Size         = new Size(90, 30);
            btnCancel.Location     = new Point(320, 195);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Click       += btnCancel_Click;

            // ── CrawlTaskEditForm ─────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(490, 245);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            StartPosition       = FormStartPosition.CenterParent;
            AcceptButton        = btnOk;
            CancelButton        = btnCancel;

            Controls.Add(lblName);
            Controls.Add(tbName);
            Controls.Add(lblTaskType);
            Controls.Add(cbTaskType);
            Controls.Add(lblBoardUrl);
            Controls.Add(tbBoardUrl);
            Controls.Add(lblKeyword);
            Controls.Add(tbKeyword);
            Controls.Add(lblMaxPages);
            Controls.Add(nudMaxPages);
            Controls.Add(cbUnlimited);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            ((System.ComponentModel.ISupportInitialize)nudMaxPages).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label            lblName;
        private TextBox          tbName;
        private Label            lblTaskType;
        private ComboBox         cbTaskType;
        private Label            lblBoardUrl;
        private TextBox          tbBoardUrl;
        private Label            lblKeyword;
        private TextBox          tbKeyword;
        private Label            lblMaxPages;
        private NumericUpDown    nudMaxPages;
        private CheckBox         cbUnlimited;
        private Button           btnOk;
        private Button           btnCancel;
    }
}
