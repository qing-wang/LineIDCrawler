namespace PTTCrawler.Forms
{
    partial class CrawlTaskManagerForm
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
            pnlTop         = new Panel();
            btnAdd         = new Button();
            btnEdit        = new Button();
            btnAbandon     = new Button();
            btnDelete      = new Button();
            btnRun         = new Button();
            btnCancel      = new Button();
            dgvTasks       = new DataGridView();
            pnlBottom      = new Panel();
            progressBar    = new ProgressBar();
            lblCurrentPost = new Label();

            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTasks).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();

            // ── pnlTop ────────────────────────────────────────
            pnlTop.Dock    = DockStyle.Top;
            pnlTop.Height  = 46;
            pnlTop.Padding = new Padding(4);
            pnlTop.Controls.Add(btnAdd);
            pnlTop.Controls.Add(btnEdit);
            pnlTop.Controls.Add(btnAbandon);
            pnlTop.Controls.Add(btnDelete);
            pnlTop.Controls.Add(btnRun);
            pnlTop.Controls.Add(btnCancel);

            // ── btnAdd ────────────────────────────────────────
            btnAdd.Text     = "新增";
            btnAdd.Size     = new Size(70, 30);
            btnAdd.Location = new Point(8, 6);
            btnAdd.Click   += btnAdd_Click;

            // ── btnEdit ───────────────────────────────────────
            btnEdit.Text     = "修改";
            btnEdit.Size     = new Size(70, 30);
            btnEdit.Location = new Point(86, 6);
            btnEdit.Enabled  = false;
            btnEdit.Click   += btnEdit_Click;

            // ── btnAbandon ────────────────────────────────────
            btnAbandon.Text     = "放棄";
            btnAbandon.Size     = new Size(70, 30);
            btnAbandon.Location = new Point(164, 6);
            btnAbandon.Enabled  = false;
            btnAbandon.Click   += btnAbandon_Click;

            // ── btnDelete ─────────────────────────────────────
            btnDelete.Text     = "刪除";
            btnDelete.Size     = new Size(70, 30);
            btnDelete.Location = new Point(242, 6);
            btnDelete.Enabled  = false;
            btnDelete.Click   += btnDelete_Click;

            // ── btnRun ────────────────────────────────────────
            btnRun.Text     = "執行";
            btnRun.Size     = new Size(70, 30);
            btnRun.Location = new Point(420, 6);
            btnRun.Enabled  = false;
            btnRun.Click   += btnRun_Click;

            // ── btnCancel ─────────────────────────────────────
            btnCancel.Text     = "取消";
            btnCancel.Size     = new Size(70, 30);
            btnCancel.Location = new Point(498, 6);
            btnCancel.Enabled  = false;
            btnCancel.Click   += btnCancel_Click;

            // ── dgvTasks ──────────────────────────────────────
            dgvTasks.Dock                  = DockStyle.Fill;
            dgvTasks.ReadOnly              = true;
            dgvTasks.AllowUserToAddRows    = false;
            dgvTasks.AllowUserToDeleteRows = false;
            dgvTasks.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgvTasks.MultiSelect           = false;
            dgvTasks.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTasks.RowHeadersVisible     = false;
            dgvTasks.SelectionChanged     += dgvTasks_SelectionChanged;

            // ── pnlBottom ─────────────────────────────────────
            pnlBottom.Dock    = DockStyle.Bottom;
            pnlBottom.Height  = 40;
            pnlBottom.Padding = new Padding(4);
            pnlBottom.Controls.Add(progressBar);
            pnlBottom.Controls.Add(lblCurrentPost);

            // ── progressBar ───────────────────────────────────
            progressBar.Location = new Point(8, 10);
            progressBar.Size     = new Size(150, 20);
            progressBar.Style    = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 0; // 靜止，執行中才動

            // ── lblCurrentPost ────────────────────────────────
            lblCurrentPost.Location  = new Point(168, 12);
            lblCurrentPost.Size      = new Size(650, 20);
            lblCurrentPost.Text      = string.Empty;
            lblCurrentPost.AutoSize  = false;

            // ── CrawlTaskManagerForm ──────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(860, 500);
            MinimumSize         = new Size(700, 380);
            Text                = "爬蟲任務管理";
            StartPosition       = FormStartPosition.CenterParent;

            Controls.Add(dgvTasks);
            Controls.Add(pnlTop);
            Controls.Add(pnlBottom);

            pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTasks).EndInit();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Panel           pnlTop;
        private Button          btnAdd;
        private Button          btnEdit;
        private Button          btnAbandon;
        private Button          btnDelete;
        private Button          btnRun;
        private Button          btnCancel;
        private DataGridView    dgvTasks;
        private Panel           pnlBottom;
        private ProgressBar     progressBar;
        private Label           lblCurrentPost;
    }
}
