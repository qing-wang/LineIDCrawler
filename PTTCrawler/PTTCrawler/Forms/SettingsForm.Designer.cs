namespace PTTCrawler.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblApiKey        = new Label();
            tbApiKey         = new TextBox();
            btnToggleVisible = new Button();
            lblModelName     = new Label();
            tbModelName      = new TextBox();
            lblTimeout       = new Label();
            nudTimeout       = new NumericUpDown();
            lblSeconds       = new Label();
            btnTest          = new Button();
            lblTestResult    = new Label();
            btnSave          = new Button();
            btnCancel        = new Button();

            ((System.ComponentModel.ISupportInitialize)nudTimeout).BeginInit();
            SuspendLayout();

            // lblApiKey
            lblApiKey.Text     = "API Key：";
            lblApiKey.Location = new Point(16, 20);
            lblApiKey.AutoSize = true;

            // tbApiKey
            tbApiKey.Location              = new Point(110, 17);
            tbApiKey.Size                  = new Size(340, 23);
            tbApiKey.UseSystemPasswordChar = true;

            // btnToggleVisible
            btnToggleVisible.Text     = "顯示";
            btnToggleVisible.Size     = new Size(50, 23);
            btnToggleVisible.Location = new Point(456, 17);
            btnToggleVisible.Click   += btnToggleVisible_Click;

            // lblModelName
            lblModelName.Text     = "模型名稱：";
            lblModelName.Location = new Point(16, 58);
            lblModelName.AutoSize = true;

            // tbModelName
            tbModelName.Text     = "gpt-4o-mini";
            tbModelName.Location = new Point(110, 55);
            tbModelName.Size     = new Size(200, 23);

            // lblTimeout
            lblTimeout.Text     = "逾時（秒）：";
            lblTimeout.Location = new Point(16, 96);
            lblTimeout.AutoSize = true;

            // nudTimeout
            nudTimeout.Minimum  = 10;
            nudTimeout.Maximum  = 300;
            nudTimeout.Value    = 60;
            nudTimeout.Location = new Point(110, 93);
            nudTimeout.Size     = new Size(70, 23);

            // lblSeconds
            lblSeconds.Text     = "秒";
            lblSeconds.Location = new Point(186, 96);
            lblSeconds.AutoSize = true;

            // btnTest
            btnTest.Text     = "測試連線";
            btnTest.Size     = new Size(90, 30);
            btnTest.Location = new Point(16, 132);
            btnTest.Click   += btnTest_Click;

            // lblTestResult
            lblTestResult.Text      = string.Empty;
            lblTestResult.Location  = new Point(116, 138);
            lblTestResult.Size      = new Size(400, 20);
            lblTestResult.AutoSize  = false;

            // btnSave
            btnSave.Text     = "儲存";
            btnSave.Size     = new Size(80, 30);
            btnSave.Location = new Point(316, 178);
            btnSave.Click   += btnSave_Click;

            // btnCancel
            btnCancel.Text     = "取消";
            btnCancel.Size     = new Size(80, 30);
            btnCancel.Location = new Point(406, 178);
            btnCancel.Click   += btnCancel_Click;

            // SettingsForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(520, 226);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            StartPosition       = FormStartPosition.CenterParent;
            Text                = "設定";

            Controls.AddRange(new Control[] {
                lblApiKey, tbApiKey, btnToggleVisible,
                lblModelName, tbModelName,
                lblTimeout, nudTimeout, lblSeconds,
                btnTest, lblTestResult,
                btnSave, btnCancel
            });

            ((System.ComponentModel.ISupportInitialize)nudTimeout).EndInit();
            ResumeLayout(false);
        }

        private Label          lblApiKey;
        private TextBox        tbApiKey;
        private Button         btnToggleVisible;
        private Label          lblModelName;
        private TextBox        tbModelName;
        private Label          lblTimeout;
        private NumericUpDown  nudTimeout;
        private Label          lblSeconds;
        private Button         btnTest;
        private Label          lblTestResult;
        private Button         btnSave;
        private Button         btnCancel;
    }
}
