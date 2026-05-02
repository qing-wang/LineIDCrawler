namespace PTTCrawler.Forms
{
    partial class PostViewForm
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
            pnlMeta      = new Panel();
            lblId        = new Label();
            lblIdValue   = new Label();
            btnOpenUrl   = new Button();
            btnProfileAnalyze = new Button();
            lblAuthor    = new Label();
            lblAuthorValue   = new Label();
            lblBoard     = new Label();
            lblBoardValue    = new Label();
            lblTitle     = new Label();
            lblTitleValue    = new Label();
            lblPostTime  = new Label();
            lblPostTimeValue = new Label();
            lblContent   = new Label();
            tbContent    = new RichTextBox();

            pnlMeta.SuspendLayout();
            SuspendLayout();

            int labelX  = 12;
            int valueX  = 90;
            int rowH    = 26;

            // ── pnlMeta ───────────────────────────────────────
            pnlMeta.Dock    = DockStyle.Top;
            pnlMeta.Height  = 160;
            pnlMeta.Padding = new Padding(8, 8, 8, 0);

            // row 0: ID + 開啟原文按鈕
            lblId.Text      = "ID：";
            lblId.Location  = new Point(labelX, 10);
            lblId.AutoSize  = true;

            lblIdValue.Location  = new Point(valueX, 10);
            lblIdValue.AutoSize  = true;

            btnOpenUrl.Text     = "開啟原貼文";
            btnOpenUrl.Size     = new Size(90, 22);
            btnOpenUrl.Location = new Point(320, 7);
            btnOpenUrl.Click   += btnOpenUrl_Click;

            btnProfileAnalyze.Text     = "人物分析";
            btnProfileAnalyze.Size     = new Size(90, 22);
            btnProfileAnalyze.Location = new Point(420, 7);
            btnProfileAnalyze.Click   += btnProfileAnalyze_Click;

            // row 1: 作者
            lblAuthor.Text     = "作者：";
            lblAuthor.Location = new Point(labelX, 10 + rowH);
            lblAuthor.AutoSize = true;

            lblAuthorValue.Location = new Point(valueX, 10 + rowH);
            lblAuthorValue.AutoSize = true;

            // row 2: 看版
            lblBoard.Text     = "看版：";
            lblBoard.Location = new Point(labelX, 10 + rowH * 2);
            lblBoard.AutoSize = true;

            lblBoardValue.Location = new Point(valueX, 10 + rowH * 2);
            lblBoardValue.AutoSize = true;

            // row 3: 標題
            lblTitle.Text     = "標題：";
            lblTitle.Location = new Point(labelX, 10 + rowH * 3);
            lblTitle.AutoSize = true;

            lblTitleValue.Location  = new Point(valueX, 10 + rowH * 3);
            lblTitleValue.Size      = new Size(540, 20);
            lblTitleValue.AutoSize  = false;
            lblTitleValue.AutoEllipsis = true;

            // row 4: 貼文時間
            lblPostTime.Text     = "貼文時間：";
            lblPostTime.Location = new Point(labelX, 10 + rowH * 4);
            lblPostTime.AutoSize = true;

            lblPostTimeValue.Location = new Point(valueX, 10 + rowH * 4);
            lblPostTimeValue.AutoSize = true;

            pnlMeta.Controls.AddRange(new Control[] {
                lblId, lblIdValue, btnOpenUrl, btnProfileAnalyze,
                lblAuthor, lblAuthorValue,
                lblBoard, lblBoardValue,
                lblTitle, lblTitleValue,
                lblPostTime, lblPostTimeValue
            });

            // ── lblContent ────────────────────────────────────
            lblContent.Text      = "本文：";
            lblContent.Location  = new Point(12, 165);
            lblContent.AutoSize  = true;

            // ── tbContent ─────────────────────────────────────
            tbContent.Location   = new Point(8, 183);
            tbContent.Size       = new Size(660, 340);
            tbContent.Anchor     = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbContent.ReadOnly   = true;
            tbContent.BackColor  = Color.White;
            tbContent.ScrollBars = RichTextBoxScrollBars.Vertical;
            tbContent.Font       = new Font("Microsoft JhengHei", 9.5F);

            // ── PostViewForm ──────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(680, 560);
            MinimumSize         = new Size(540, 440);
            Text                = "貼文檢視";
            StartPosition       = FormStartPosition.CenterParent;

            Controls.Add(pnlMeta);
            Controls.Add(lblContent);
            Controls.Add(tbContent);

            pnlMeta.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private Panel   pnlMeta;
        private Label   lblId;
        private Label   lblIdValue;
        private Button  btnOpenUrl;
        private Button  btnProfileAnalyze;
        private Label   lblAuthor;
        private Label   lblAuthorValue;
        private Label   lblBoard;
        private Label   lblBoardValue;
        private Label   lblTitle;
        private Label   lblTitleValue;
        private Label   lblPostTime;
        private Label   lblPostTimeValue;
        private Label   lblContent;
        private RichTextBox tbContent;
    }
}
