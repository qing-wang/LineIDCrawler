namespace PTTCrawler.Forms
{
    partial class AuthorProfileResultForm
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
            tlpProfile           = new TableLayoutPanel();
            pnlButtons           = new Panel();
            btnCopy              = new Button();
            btnClose             = new Button();
            lblGenderKey         = new Label();
            lblGenderValue       = new Label();
            lblAgeKey            = new Label();
            lblAgeValue          = new Label();
            lblAreaKey           = new Label();
            lblAreaValue         = new Label();
            lblInterestsKey      = new Label();
            lblInterestsValue    = new Label();
            lblRelationshipKey   = new Label();
            lblRelationshipValue = new Label();
            lblOccupationKey     = new Label();
            lblOccupationValue   = new Label();

            tlpProfile.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();

            // ── tlpProfile ────────────────────────────────────
            tlpProfile.Dock        = DockStyle.Fill;
            tlpProfile.Padding     = new Padding(16, 12, 16, 4);
            tlpProfile.ColumnCount = 2;
            tlpProfile.RowCount    = 6;
            tlpProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            tlpProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            for (int i = 0; i < 6; i++)
                tlpProfile.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            void AddRow(int row, Label key, string keyText, Label val)
            {
                key.Text      = keyText;
                key.Dock      = DockStyle.Fill;
                key.TextAlign = ContentAlignment.MiddleLeft;
                key.Font      = new Font(key.Font, FontStyle.Bold);
                tlpProfile.Controls.Add(key, 0, row);

                val.Dock         = DockStyle.Fill;
                val.TextAlign    = ContentAlignment.MiddleLeft;
                val.AutoEllipsis = true;
                tlpProfile.Controls.Add(val, 1, row);
            }

            AddRow(0, lblGenderKey,       "性別",      lblGenderValue);
            AddRow(1, lblAgeKey,          "年紀",      lblAgeValue);
            AddRow(2, lblAreaKey,         "居住地區",  lblAreaValue);
            AddRow(3, lblInterestsKey,    "興趣",      lblInterestsValue);
            AddRow(4, lblRelationshipKey, "感情狀態",  lblRelationshipValue);
            AddRow(5, lblOccupationKey,   "職業/身份", lblOccupationValue);

            // ── pnlButtons ────────────────────────────────────
            pnlButtons.Dock    = DockStyle.Bottom;
            pnlButtons.Height  = 44;
            pnlButtons.Padding = new Padding(12, 7, 12, 7);

            btnCopy.Text     = "複製全部";
            btnCopy.Size     = new Size(90, 30);
            btnCopy.Location = new Point(12, 7);
            btnCopy.Click   += btnCopy_Click;

            btnClose.Text     = "關閉";
            btnClose.Size     = new Size(80, 30);
            btnClose.Location = new Point(370, 7);
            btnClose.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click   += (_, _) => Close();

            pnlButtons.Controls.Add(btnCopy);
            pnlButtons.Controls.Add(btnClose);

            // ── Form ──────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(480, 320);
            MinimumSize         = new Size(420, 300);
            Text                = "人物分析結果";
            StartPosition       = FormStartPosition.CenterParent;

            Controls.Add(tlpProfile);
            Controls.Add(pnlButtons);

            tlpProfile.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private TableLayoutPanel tlpProfile;
        private Panel   pnlButtons;
        private Button  btnCopy;
        private Button  btnClose;
        private Label   lblGenderKey,       lblGenderValue;
        private Label   lblAgeKey,          lblAgeValue;
        private Label   lblAreaKey,         lblAreaValue;
        private Label   lblInterestsKey,    lblInterestsValue;
        private Label   lblRelationshipKey, lblRelationshipValue;
        private Label   lblOccupationKey,   lblOccupationValue;
    }
}
