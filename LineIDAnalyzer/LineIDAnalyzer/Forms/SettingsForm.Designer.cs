namespace LineIDAnalyzer.Forms
{
    partial class SettingsForm
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
            lblApiKey              = new Label();
            tbApiKey               = new TextBox();
            btnToggleApiKeyVisibility = new Button();
            lblModelName           = new Label();
            tbModelName            = new TextBox();
            lblModelHint           = new Label();
            btnTestConnection      = new Button();
            btnSave                = new Button();
            btnCancel              = new Button();
            tableLayout            = new TableLayoutPanel();

            tableLayout.SuspendLayout();
            SuspendLayout();

            // ── tableLayout ──────────────────────────────────
            tableLayout.ColumnCount = 3;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tableLayout.RowCount = 5;
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayout.Dock    = DockStyle.Fill;
            tableLayout.Padding = new Padding(12, 12, 12, 8);

            // ── lblApiKey ─────────────────────────────────────
            lblApiKey.Text      = "API Key：";
            lblApiKey.Dock      = DockStyle.Fill;
            lblApiKey.TextAlign = ContentAlignment.MiddleLeft;
            tableLayout.Controls.Add(lblApiKey, 0, 0);

            // ── tbApiKey ──────────────────────────────────────
            tbApiKey.Dock                  = DockStyle.Fill;
            tbApiKey.UseSystemPasswordChar = true;
            tbApiKey.Margin                = new Padding(0, 6, 4, 6);
            tableLayout.Controls.Add(tbApiKey, 1, 0);

            // ── btnToggleApiKeyVisibility ─────────────────────
            btnToggleApiKeyVisibility.Text    = "顯示";
            btnToggleApiKeyVisibility.Dock    = DockStyle.Fill;
            btnToggleApiKeyVisibility.Margin  = new Padding(0, 6, 0, 6);
            btnToggleApiKeyVisibility.Click  += btnToggleApiKeyVisibility_Click;
            tableLayout.Controls.Add(btnToggleApiKeyVisibility, 2, 0);

            // ── lblModelName ──────────────────────────────────
            lblModelName.Text      = "模型名稱：";
            lblModelName.Dock      = DockStyle.Fill;
            lblModelName.TextAlign = ContentAlignment.MiddleLeft;
            tableLayout.Controls.Add(lblModelName, 0, 1);

            // ── tbModelName ───────────────────────────────────
            tbModelName.Text   = "gpt-4o-mini";
            tbModelName.Dock   = DockStyle.Fill;
            tbModelName.Margin = new Padding(0, 6, 4, 6);
            tableLayout.SetColumnSpan(tbModelName, 2);
            tableLayout.Controls.Add(tbModelName, 1, 1);

            // ── lblModelHint ──────────────────────────────────
            lblModelHint.Text      = "例如：gpt-4o、gpt-4o-mini、gpt-3.5-turbo";
            lblModelHint.ForeColor = Color.Gray;
            lblModelHint.Font      = new Font(Font.FontFamily, 8f);
            lblModelHint.Dock      = DockStyle.Fill;
            lblModelHint.Margin    = new Padding(110, 0, 0, 0);
            tableLayout.SetColumnSpan(lblModelHint, 3);
            tableLayout.Controls.Add(lblModelHint, 0, 2);

            // ── (separator row) ───────────────────────────────
            tableLayout.Controls.Add(new Label(), 0, 3);

            // ── button row ────────────────────────────────────
            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents  = false
            };
            tableLayout.SetColumnSpan(btnPanel, 3);
            tableLayout.Controls.Add(btnPanel, 0, 4);

            btnCancel.Text          = "取消";
            btnCancel.Width         = 80;
            btnCancel.Height        = 30;
            btnCancel.Margin        = new Padding(4, 4, 0, 4);
            btnCancel.Click        += btnCancel_Click;

            btnSave.Text            = "儲存";
            btnSave.Width           = 80;
            btnSave.Height          = 30;
            btnSave.Margin          = new Padding(4, 4, 0, 4);
            btnSave.Click          += btnSave_Click;

            btnTestConnection.Text   = "測試連線";
            btnTestConnection.Width  = 90;
            btnTestConnection.Height = 30;
            btnTestConnection.Margin = new Padding(4, 4, 0, 4);
            btnTestConnection.Click += btnTestConnection_Click;

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnTestConnection);

            // ── SettingsForm ──────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(440, 158);
            Controls.Add(tableLayout);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            StartPosition       = FormStartPosition.CenterParent;
            Text                = "設定";

            tableLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Label     lblApiKey;
        private TextBox   tbApiKey;
        private Button    btnToggleApiKeyVisibility;
        private Label     lblModelName;
        private TextBox   tbModelName;
        private Label     lblModelHint;
        private Button    btnTestConnection;
        private Button    btnSave;
        private Button    btnCancel;
        private TableLayoutPanel tableLayout;
    }
}
