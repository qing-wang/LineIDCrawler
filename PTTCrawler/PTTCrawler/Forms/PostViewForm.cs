using PTTCrawler.Business;
using PTTCrawler.Data;
using PTTCrawler.Models;

namespace PTTCrawler.Forms
{
    public partial class PostViewForm : Form
    {
        private readonly Post            _post;
        private readonly DatabaseManager _db;

        public PostViewForm(Post post, DatabaseManager db)
        {
            InitializeComponent();
            _post = post;
            _db   = db;
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

        private async void btnProfileAnalyze_Click(object sender, EventArgs e)
        {
            var apiKey = _db.LoadApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("尚未設定 API Key，請先至「設定」頁面輸入。", "缺少設定",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var modelName = _db.GetSetting("model_name") ?? "gpt-4o-mini";
            var settings  = new AppSettings { ApiKey = apiKey, ModelName = modelName };
            var analyzer  = new AuthorProfileAnalyzer(settings);

            var request = new AuthorProfileRequest
            {
                Title    = _post.Title,
                AuthorId = _post.AuthorId,
                Nickname = _post.AuthorNick,
                Body     = _post.Content ?? string.Empty
            };

            btnProfileAnalyze.Enabled = false;
            btnProfileAnalyze.Text    = "分析中…";
            try
            {
                var profile = await analyzer.AnalyzeAsync(request);
                if (!profile.IsSuccess)
                {
                    MessageBox.Show($"人物分析失敗：{profile.ErrorMessage}", "錯誤",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _db.SaveProfileHistory(_post.Id, request, profile);

                using var form = new AuthorProfileResultForm(profile);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"人物分析時發生錯誤：{ex.Message}", "錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnProfileAnalyze.Enabled = true;
                btnProfileAnalyze.Text    = "人物分析";
            }
        }
    }
}
