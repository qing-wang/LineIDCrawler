namespace PTTCrawler.Forms
{
    partial class PTTCrawlerUI
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
            pnlButtons     = new Panel();
            btnCrawlTasks  = new Button();
            btnViewPosts   = new Button();
            lblConsoleLog  = new Label();
            tbConsoleLog   = new RichTextBox();
            statusStrip    = new StatusStrip();
            statusLabel    = new ToolStripStatusLabel();

            pnlButtons.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // ── pnlButtons ────────────────────────────────────
            pnlButtons.Controls.Add(btnCrawlTasks);
            pnlButtons.Controls.Add(btnViewPosts);
            pnlButtons.Dock    = DockStyle.Top;
            pnlButtons.Height  = 46;
            pnlButtons.Padding = new Padding(4);

            // ── btnCrawlTasks ─────────────────────────────────
            btnCrawlTasks.Text     = "爬蟲任務";
            btnCrawlTasks.Size     = new Size(90, 30);
            btnCrawlTasks.Location = new Point(8, 6);
            btnCrawlTasks.Click   += btnCrawlTasks_Click;

            // ── btnViewPosts ──────────────────────────────────
            btnViewPosts.Text     = "檢視貼文";
            btnViewPosts.Size     = new Size(90, 30);
            btnViewPosts.Location = new Point(106, 6);
            btnViewPosts.Click   += btnViewPosts_Click;

            // ── lblConsoleLog ─────────────────────────────────
            lblConsoleLog.Text      = "系統日誌：";
            lblConsoleLog.Location  = new Point(8, 52);
            lblConsoleLog.AutoSize  = true;

            // ── tbConsoleLog（RichTextBox）────────────────────
            tbConsoleLog.Location   = new Point(8, 70);
            tbConsoleLog.Size       = new Size(764, 480);
            tbConsoleLog.Anchor     = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbConsoleLog.ReadOnly   = true;
            tbConsoleLog.BackColor  = Color.White;
            tbConsoleLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            tbConsoleLog.Font       = new Font("Consolas", 9F);

            // ── statusStrip ───────────────────────────────────
            statusLabel.Text      = "就緒";
            statusLabel.Spring    = true;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusStrip.Items.Add(statusLabel);

            // ── PTTCrawlerUI ──────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(800, 600);
            MinimumSize         = new Size(620, 480);
            Text                = "PTT Crawler";
            StartPosition       = FormStartPosition.CenterScreen;

            Controls.Add(pnlButtons);
            Controls.Add(lblConsoleLog);
            Controls.Add(tbConsoleLog);
            Controls.Add(statusStrip);

            pnlButtons.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private Panel               pnlButtons;
        private Button              btnCrawlTasks;
        private Button              btnViewPosts;
        private Label               lblConsoleLog;
        private RichTextBox         tbConsoleLog;
        private StatusStrip         statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}
