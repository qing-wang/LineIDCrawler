using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class PostViewForm : Form
    {
        private readonly Post _post;

        public PostViewForm(Post post)
        {
            InitializeComponent();
            _post = post;
            LoadPost();
        }

        private void LoadPost()
        {
            // 從 post ID 組出原貼文網址
            // board 格式：AllTogether，post.Id 格式：M.1777589218.A.656
            // 網址：https://www.ptt.cc/bbs/{board}/{postId}.html
            string boardName = _post.Board ?? string.Empty;
            lblIdValue.Text        = _post.Id;
            lblAuthorValue.Text    = string.IsNullOrEmpty(_post.AuthorNick)
                ? _post.AuthorId ?? string.Empty
                : $"{_post.AuthorId} ({_post.AuthorNick})";
            lblBoardValue.Text     = boardName;
            lblTitleValue.Text     = _post.Title ?? string.Empty;
            lblPostTimeValue.Text  = _post.PostTime ?? string.Empty;
            tbContent.Text         = _post.Content ?? string.Empty;
            tbContent.SelectionStart = 0;
        }

        private void btnOpenUrl_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_post.Board) || string.IsNullOrEmpty(_post.Id))
            {
                MessageBox.Show("無法組出原貼文網址（缺少看版或貼文 ID）。",
                    "無法開啟", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var url = $"https://www.ptt.cc/bbs/{_post.Board}/{_post.Id}.html";
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url)
                    { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法開啟瀏覽器：{ex.Message}", "錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
