namespace PTTCrawler.Forms
{
    partial class PostBrowserForm
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
            pnlFilter    = new Panel();
            rbByBoard    = new RadioButton();
            cbBoards     = new ComboBox();
            rbByTask     = new RadioButton();
            cbTasks      = new ComboBox();
            lblSort      = new Label();
            rbAscending  = new RadioButton();
            rbDescending = new RadioButton();
            btnLoad      = new Button();
            dgvPosts     = new DataGridView();
            pnlPaging    = new Panel();
            btnFirst     = new Button();
            btnPrev      = new Button();
            lblPageInfo  = new Label();
            btnNext      = new Button();
            btnLast      = new Button();

            pnlFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPosts).BeginInit();
            pnlPaging.SuspendLayout();
            SuspendLayout();

            // ── pnlFilter ─────────────────────────────────────
            pnlFilter.Dock    = DockStyle.Top;
            pnlFilter.Height  = 72;
            pnlFilter.Padding = new Padding(8, 6, 8, 4);
            pnlFilter.Controls.AddRange(new Control[] {
                rbByBoard, cbBoards, rbByTask, cbTasks,
                lblSort, rbAscending, rbDescending, btnLoad
            });

            // Row 1: 依看版
            rbByBoard.Text     = "依看版：";
            rbByBoard.Location = new Point(8, 8);
            rbByBoard.AutoSize = true;
            rbByBoard.Checked  = true;
            rbByBoard.CheckedChanged += rbByBoard_CheckedChanged;

            cbBoards.Location      = new Point(82, 6);
            cbBoards.Size          = new Size(200, 23);
            cbBoards.DropDownStyle = ComboBoxStyle.DropDownList;

            rbByTask.Text     = "依爬蟲任務：";
            rbByTask.Location = new Point(300, 8);
            rbByTask.AutoSize = true;

            cbTasks.Location      = new Point(394, 6);
            cbTasks.Size          = new Size(300, 23);
            cbTasks.DropDownStyle = ComboBoxStyle.DropDownList;

            // Row 2: 排序 + 載入
            lblSort.Text     = "排序：";
            lblSort.Location = new Point(8, 40);
            lblSort.AutoSize = true;

            rbAscending.Text     = "由舊到新";
            rbAscending.Location = new Point(54, 38);
            rbAscending.AutoSize = true;
            rbAscending.Checked  = true;
            rbAscending.CheckedChanged += rbAscending_CheckedChanged;

            rbDescending.Text     = "由新到舊";
            rbDescending.Location = new Point(140, 38);
            rbDescending.AutoSize = true;

            btnLoad.Text     = "載入";
            btnLoad.Size     = new Size(70, 26);
            btnLoad.Location = new Point(240, 36);
            btnLoad.Click   += btnLoad_Click;

            // ── dgvPosts ──────────────────────────────────────
            dgvPosts.Dock                  = DockStyle.Fill;
            dgvPosts.ReadOnly              = true;
            dgvPosts.AllowUserToAddRows    = false;
            dgvPosts.AllowUserToDeleteRows = false;
            dgvPosts.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgvPosts.MultiSelect           = false;
            dgvPosts.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPosts.RowHeadersVisible     = false;
            dgvPosts.CellDoubleClick      += dgvPosts_CellDoubleClick;

            // ── pnlPaging ─────────────────────────────────────
            pnlPaging.Dock    = DockStyle.Bottom;
            pnlPaging.Height  = 38;
            pnlPaging.Padding = new Padding(6, 6, 6, 0);
            pnlPaging.Controls.AddRange(new Control[] { btnFirst, btnPrev, lblPageInfo, btnNext, btnLast });

            btnFirst.Text     = "|◄";
            btnFirst.Size     = new Size(40, 26);
            btnFirst.Location = new Point(6, 6);
            btnFirst.Enabled  = false;
            btnFirst.Click   += btnFirst_Click;

            btnPrev.Text     = "◄";
            btnPrev.Size     = new Size(40, 26);
            btnPrev.Location = new Point(50, 6);
            btnPrev.Enabled  = false;
            btnPrev.Click   += btnPrev_Click;

            lblPageInfo.Text      = string.Empty;
            lblPageInfo.Location  = new Point(100, 10);
            lblPageInfo.Size      = new Size(300, 20);
            lblPageInfo.AutoSize  = false;

            btnNext.Text     = "►";
            btnNext.Size     = new Size(40, 26);
            btnNext.Location = new Point(408, 6);
            btnNext.Enabled  = false;
            btnNext.Click   += btnNext_Click;

            btnLast.Text     = "►|";
            btnLast.Size     = new Size(40, 26);
            btnLast.Location = new Point(452, 6);
            btnLast.Enabled  = false;
            btnLast.Click   += btnLast_Click;

            // ── PostBrowserForm ───────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(1280, 720);
            MinimumSize         = new Size(900, 500);
            Text                = "貼文瀏覽";
            StartPosition       = FormStartPosition.CenterParent;

            Controls.Add(dgvPosts);
            Controls.Add(pnlFilter);
            Controls.Add(pnlPaging);

            pnlFilter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPosts).EndInit();
            pnlPaging.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Panel           pnlFilter;
        private RadioButton     rbByBoard;
        private ComboBox        cbBoards;
        private RadioButton     rbByTask;
        private ComboBox        cbTasks;
        private Label           lblSort;
        private RadioButton     rbAscending;
        private RadioButton     rbDescending;
        private Button          btnLoad;
        private DataGridView    dgvPosts;
        private Panel           pnlPaging;
        private Button          btnFirst;
        private Button          btnPrev;
        private Label           lblPageInfo;
        private Button          btnNext;
        private Button          btnLast;
    }
}
