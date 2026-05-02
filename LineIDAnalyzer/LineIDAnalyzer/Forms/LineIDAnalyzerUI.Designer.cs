namespace LineIDAnalyzer.Forms
{
    partial class LineIDAnalyzerUI
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
            // ── Controls ─────────────────────────────────────
            pnlButtons        = new Panel();
            btnAnalyze        = new Button();
            btnSettings       = new Button();
            btnClear          = new Button();
            btnCopyResult     = new Button();
            btnCancelAnalysis = new Button();
            btnRunTests       = new Button();

            lblInputText     = new Label();
            tbInputText      = new TextBox();
            lblAnalysisResult = new Label();
            tbAnalysisResult = new TextBox();
            lblConsoleLog    = new Label();
            tbConsoleLog     = new RichTextBox();

            statusStrip      = new StatusStrip();
            statusLabel      = new ToolStripStatusLabel();

            pnlButtons.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // ── pnlButtons（頂部按鈕群）──────────────────────
            pnlButtons.Dock    = DockStyle.Top;
            pnlButtons.Height  = 44;
            pnlButtons.Padding = new Padding(8, 6, 8, 4);
            pnlButtons.Controls.Add(btnCancelAnalysis);
            pnlButtons.Controls.Add(btnRunTests);
            pnlButtons.Controls.Add(btnCopyResult);
            pnlButtons.Controls.Add(btnClear);
            pnlButtons.Controls.Add(btnSettings);
            pnlButtons.Controls.Add(btnAnalyze);

            // ── btnAnalyze ────────────────────────────────────
            btnAnalyze.Text     = "分析";
            btnAnalyze.Size     = new Size(80, 30);
            btnAnalyze.Location = new Point(8, 6);
            btnAnalyze.Click   += btnAnalyze_Click;

            // ── btnSettings ───────────────────────────────────
            btnSettings.Text     = "設定";
            btnSettings.Size     = new Size(80, 30);
            btnSettings.Location = new Point(96, 6);
            btnSettings.Click   += btnSettings_Click;

            // ── btnClear ──────────────────────────────────────
            btnClear.Text     = "清除";
            btnClear.Size     = new Size(80, 30);
            btnClear.Location = new Point(184, 6);
            btnClear.Click   += btnClear_Click;

            // ── btnCopyResult ─────────────────────────────────
            btnCopyResult.Text     = "複製結果";
            btnCopyResult.Size     = new Size(90, 30);
            btnCopyResult.Location = new Point(272, 6);
            btnCopyResult.Click   += btnCopyResult_Click;

            // ── btnCancelAnalysis ─────────────────────────────
            btnCancelAnalysis.Text     = "取消";
            btnCancelAnalysis.Size     = new Size(80, 30);
            btnCancelAnalysis.Location = new Point(370, 6);
            btnCancelAnalysis.Enabled  = false;
            btnCancelAnalysis.Click   += btnCancelAnalysis_Click;

            // ── btnRunTests ───────────────────────────────────
            btnRunTests.Text     = "執行測試";
            btnRunTests.Size     = new Size(90, 30);
            btnRunTests.Location = new Point(458, 6);
            btnRunTests.Click   += btnRunTests_Click;

            // ── lblInputText ──────────────────────────────────
            lblInputText.Text     = "待分析文字：";
            lblInputText.Location = new Point(8, 52);
            lblInputText.AutoSize = true;

            // ── tbInputText ───────────────────────────────────
            tbInputText.Multiline     = true;
            tbInputText.ScrollBars    = ScrollBars.Vertical;
            tbInputText.Location      = new Point(8, 70);
            tbInputText.Size          = new Size(764, 120);
            tbInputText.Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbInputText.AcceptsReturn = true;

            // ── lblAnalysisResult ─────────────────────────────
            lblAnalysisResult.Text     = "分析結果（Line ID）：";
            lblAnalysisResult.Location = new Point(8, 198);
            lblAnalysisResult.AutoSize = true;

            // ── tbAnalysisResult ──────────────────────────────
            tbAnalysisResult.Multiline  = true;
            tbAnalysisResult.ScrollBars = ScrollBars.Vertical;
            tbAnalysisResult.ReadOnly   = true;
            tbAnalysisResult.BackColor  = SystemColors.Window;
            tbAnalysisResult.Location   = new Point(8, 216);
            tbAnalysisResult.Size       = new Size(764, 80);
            tbAnalysisResult.Anchor     = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ── lblConsoleLog ─────────────────────────────────
            lblConsoleLog.Text     = "Console Log：";
            lblConsoleLog.Location = new Point(8, 304);
            lblConsoleLog.AutoSize = true;

            // ── tbConsoleLog（RichTextBox）────────────────────
            tbConsoleLog.Location  = new Point(8, 322);
            tbConsoleLog.Size      = new Size(764, 260);
            tbConsoleLog.Anchor    = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbConsoleLog.ReadOnly  = true;
            tbConsoleLog.BackColor = Color.White;
            tbConsoleLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            tbConsoleLog.Font      = new Font("Consolas", 9F);

            // ── statusStrip ───────────────────────────────────
            statusLabel.Text      = "就緒";
            statusLabel.Spring    = true;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusStrip.Items.Add(statusLabel);

            // ── LineIDAnalyzerUI ──────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(800, 620);
            MinimumSize         = new Size(620, 520);
            Text                = "LineIDAnalyzer";
            StartPosition       = FormStartPosition.CenterScreen;

            Controls.Add(pnlButtons);
            Controls.Add(lblInputText);
            Controls.Add(tbInputText);
            Controls.Add(lblAnalysisResult);
            Controls.Add(tbAnalysisResult);
            Controls.Add(lblConsoleLog);
            Controls.Add(tbConsoleLog);
            Controls.Add(statusStrip);

            pnlButtons.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // ── Fields ───────────────────────────────────────────
        private Panel             pnlButtons;
        private Button            btnAnalyze;
        private Button            btnSettings;
        private Button            btnClear;
        private Button            btnCopyResult;
        private Button            btnCancelAnalysis;
        private Button            btnRunTests;
        private Label             lblInputText;
        private TextBox           tbInputText;
        private Label             lblAnalysisResult;
        private TextBox           tbAnalysisResult;
        private Label             lblConsoleLog;
        internal RichTextBox      tbConsoleLog;
        private StatusStrip       statusStrip;
        private ToolStripStatusLabel statusLabel;
    }
}
