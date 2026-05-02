using HtmlAgilityPack;
using PTTCrawler.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PTTCrawler.Business
{
    /// <summary>
    /// 剖析 PTT 看版列表頁及貼文內容頁的 HTML。
    /// </summary>
    public static class PttHtmlParser
    {
        private static readonly Regex PostIdRegex =
            new(@"/(M\.\d+\.A\.[A-Z0-9]+)\.html", RegexOptions.Compiled);

        private static HtmlAgilityPack.HtmlDocument LoadDoc(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        // ── 看版列表頁 ────────────────────────────────────────

        /// <summary>
        /// 從列表頁 HTML 擷取所有有效貼文列（公告之前的）。
        /// 已刪除（無 a 標籤）的貼文列直接略過。
        /// </summary>
        public static List<PostListItem> ParseBoardList(string html)
        {
            var doc  = LoadDoc(html);
            var items = new List<PostListItem>();

            var rEntNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'r-ent')]");
            if (rEntNodes == null) return items;

            // 找出 r-list-sep 的位置（文件中的節點順序）
            var sepNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'r-list-sep')]");

            foreach (var ent in rEntNodes)
            {
                // 公告判斷：若此 r-ent 出現在 r-list-sep 之後則略過
                if (sepNode != null && IsNodeAfter(ent, sepNode))
                    continue;

                var aNode = ent.SelectSingleNode(".//div[@class='title']/a");
                if (aNode == null) continue;   // 已刪除

                var href  = HtmlEntity.DeEntitize(aNode.GetAttributeValue("href", ""));
                var title = HtmlEntity.DeEntitize(aNode.InnerText.Trim());
                var postId = ExtractPostId(href);
                if (string.IsNullOrEmpty(postId)) continue;

                var nrecNode = ent.SelectSingleNode(".//div[@class='nrec']");
                var nrec     = nrecNode?.InnerText.Trim() ?? "";

                items.Add(new PostListItem
                {
                    PostId = postId,
                    Title  = title,
                    Href   = href,
                    Nrec   = nrec
                });
            }
            return items;
        }

        /// <summary>
        /// 從列表頁 HTML 取得「上頁」的相對 URL，若已是第一頁則回傳 null。
        /// </summary>
        public static string? GetPrevPageUrl(string html)
        {
            var doc   = LoadDoc(html);
            var nodes = doc.DocumentNode.SelectNodes("//a[@class='btn wide']");
            if (nodes == null) return null;
            foreach (var node in nodes)
            {
                if (node.InnerText.Contains("上頁"))
                {
                    var href = node.GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(href)) return null;
                    return HtmlEntity.DeEntitize(href);   // 解碼 &amp; → &
                }
            }
            return null;
        }

        // ── 貼文內容頁 ────────────────────────────────────────

        /// <summary>
        /// 剖析貼文內容頁，回傳 Post 模型（不含 Id，由呼叫端填入）。
        /// </summary>
        public static Post ParsePostContent(string html, string postId, string postUrl)
        {
            var doc = LoadDoc(html);

            var post = new Post
            {
                Id        = postId,
                CrawledAt = DateTime.Now
            };

            // 擷取 article-meta-value 順序：作者、看版、標題、時間
            var metaValues = doc.DocumentNode
                .SelectNodes("//span[@class='article-meta-value']");

            if (metaValues != null && metaValues.Count >= 4)
            {
                var authorFull = HtmlEntity.DeEntitize(metaValues[0].InnerText.Trim());
                ParseAuthor(authorFull, out var authorId, out var authorNick);
                post.AuthorId   = authorId;
                post.AuthorNick = authorNick;
                post.Board    = HtmlEntity.DeEntitize(metaValues[1].InnerText.Trim());
                post.Title    = HtmlEntity.DeEntitize(metaValues[2].InnerText.Trim());
                post.PostTime = HtmlEntity.DeEntitize(metaValues[3].InnerText.Trim());
            }

            // 本文：main-content 裡、不在任何 div/span 之內的文字節點
            var mainContent = doc.DocumentNode.SelectSingleNode("//div[@id='main-content']");
            if (mainContent != null)
                post.Content = ExtractBodyText(mainContent);

            return post;
        }

        // ── 輔助 ──────────────────────────────────────────────

        public static string? ExtractPostId(string href)
        {
            var m = PostIdRegex.Match(href);
            return m.Success ? m.Groups[1].Value : null;
        }

        private static void ParseAuthor(string raw, out string? id, out string? nick)
        {
            // 格式：snksohot (Sunny)
            var m = Regex.Match(raw, @"^(\S+)\s*\((.+)\)$");
            if (m.Success)
            {
                id   = m.Groups[1].Value;
                nick = m.Groups[2].Value;
            }
            else
            {
                id   = raw;
                nick = null;
            }
        }

        private static string ExtractBodyText(HtmlNode mainContent)
        {
            var sb = new StringBuilder();
            foreach (var node in mainContent.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    var text = HtmlEntity.DeEntitize(node.InnerText);
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text.Trim());
                }
            }
            return sb.ToString().Trim();
        }

        private static bool IsNodeAfter(HtmlNode target, HtmlNode reference)
        {
            // 比較在文件中的位置（StreamPosition / Line / LinePosition）
            return target.StreamPosition > reference.StreamPosition;
        }
    }

    public class PostListItem
    {
        public string PostId { get; set; } = string.Empty;
        public string Title  { get; set; } = string.Empty;
        public string Href   { get; set; } = string.Empty;
        public string Nrec   { get; set; } = string.Empty;
    }
}
