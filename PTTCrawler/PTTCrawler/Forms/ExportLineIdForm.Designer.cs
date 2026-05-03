namespace PTTCrawler.Forms
{
    partial class ExportLineIdForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblScope    = new Label();
            rbNewOnly   = new RadioButton();
            rbAll       = new RadioButton();
            lblFile     = new Label();
            txtFilePath = new TextBox();
            btnBrowse   = new Button();
            btnOk       = new Button();
            btnCancel   = new Button();
            SuspendLayout();

            // lblScope
            lblScope.Text     = "匯出範圍：";
            lblScope.Location = new Point(12, 16);
            lblScope.AutoSize = true;
            lblScope.Font     = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);

            // rbNewOnly
            rbNewOnly.Text            = "只匯出未匯出過的 Line ID";
            rbNewOnly.Location        = new Point(28, 40);
            rbNewOnly.AutoSize        = true;
            rbNewOnly.Checked         = true;
            rbNewOnly.CheckedChanged += rbNewOnly_CheckedChanged;

            // rbAll
            rbAll.Text            = "全部匯出";
            rbAll.Location        = new Point(28, 66);
            rbAll.AutoSize        = true;
            rbAll.CheckedChanged += rbAll_CheckedChanged;

            // lblFile
            lblFile.Text     = "輸出檔案：";
            lblFile.Location = new Point(12, 106);
            lblFile.AutoSize = true;

            // txtFilePath
            txtFilePath.Location     = new Point(28, 126);
            txtFilePath.Size         = new Size(370, 23);
            txtFilePath.Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFilePath.TextChanged += txtFilePath_TextChanged;

            // btnBrowse
            btnBrowse.Text     = "瀏覽…";
            btnBrowse.Size     = new Size(70, 23);
            btnBrowse.Location = new Point(406, 126);
            btnBrowse.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Click   += btnBrowse_Click;

            // btnOk
            btnOk.Text     = "匯出";
            btnOk.Size     = new Size(80, 28);
            btnOk.Location = new Point(246, 168);
            btnOk.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Enabled  = false;
            btnOk.Click   += btnOk_Click;

            // btnCancel
            btnCancel.Text     = "取消";
            btnCancel.Size     = new Size(80, 28);
            btnCancel.Location = new Point(334, 168);
            btnCancel.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Click   += btnCancel_Click;

            // ExportLineIdForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(490, 210);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            StartPosition       = FormStartPosition.CenterParent;
            Text                = "匯出 Line ID";
            AcceptButton        = btnOk;
            CancelButton        = btnCancel;

            Controls.AddRange(new Control[]
            {
                lblScope, rbNewOnly, rbAll,
                lblFile, txtFilePath, btnBrowse,
                btnOk, btnCancel
            });

            ResumeLayout(false);
        }

        private Label       lblScope;
        private RadioButton rbNewOnly;
        private RadioButton rbAll;
        private Label       lblFile;
        private TextBox     txtFilePath;
        private Button      btnBrowse;
        private Button      btnOk;
        private Button      btnCancel;
    }
}
