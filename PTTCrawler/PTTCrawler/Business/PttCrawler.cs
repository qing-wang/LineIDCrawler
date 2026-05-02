using System.Net;
using PTTCrawler.Data;
using PTTCrawler.Logging;
using PTTCrawler.Models;

namespace PTTCrawler.Business
{
    public class CrawlProgressInfo
    {
        public int     PagesDone     { get; set; }
        public int     NewPosts      { get; set; }
        public int     Skipped       { get; set; }
        public string  CurrentTitle  { get; set; } = string.Empty;
    }

    public class PttCrawler
    {
        private static readonly Random _rng = new();
        private readonly DatabaseManager  _db;
        private readonly HttpClient       _http;

        public PttCrawler(DatabaseManager db)
        {
            _db   = db;
            _http = CreateHttpClient();
        }

        private static HttpClient CreateHttpClient()
        {
            var cookies = new CookieContainer();
            cookies.Add(new Uri("https://www.ptt.cc"), new Cookie("over18", "1"));

            var handler = new SocketsHttpHandler
            {
                CookieContainer        = cookies,
                UseCookies             = true,
                AutomaticDecompression = DecompressionMethods.GZip
                                       | DecompressionMethods.Deflate
                                       | DecompressionMethods.Brotli,
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    EnabledSslProtocols =
                        System.Security.Authentication.SslProtocols.Tls12 |
                        System.Security.Authentication.SslProtocols.Tls13
                },
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Connection",       "keep-alive");

            return client;
        }

        /// <summary>
        /// 執行指定爬蟲任務。
        /// </summary>
        public async Task RunAsync(
            CrawlTask task,
            IProgress<CrawlProgressInfo>? progress = null,
            CancellationToken ct = default)
        {
            AppLogger.Info($"開始執行爬蟲任務「{task.Name}」。");
            int execId   = _db.InsertExecution(task.Id);
            int newPosts = 0;
            int skipped  = 0;

            try
            {
                // 決定起始 URL
                string startUrl = BuildStartUrl(task);
                AppLogger.Debug($"起始網址：{startUrl}");

                string? currentUrl = startUrl;
                int pagesDone = 0;

                while (!string.IsNullOrEmpty(currentUrl))
                {
                    ct.ThrowIfCancellationRequested();

                    AppLogger.Debug($"正在爬取頁面（第 {pagesDone + 1} 頁）：{currentUrl}");
                    string html = await FetchAsync(currentUrl, ct);

                    var items = PttHtmlParser.ParseBoardList(html);
                    AppLogger.Debug($"本頁偵測到 {items.Count} 個貼文列。");

                    foreach (var item in items)
                    {
                        ct.ThrowIfCancellationRequested();

                        progress?.Report(new CrawlProgressInfo
                        {
                            PagesDone    = pagesDone,
                            NewPosts     = newPosts,
                            Skipped      = skipped,
                            CurrentTitle = item.Title
                        });

                        if (_db.PostExists(item.PostId))
                        {
                            skipped++;
                            AppLogger.Debug($"已存在，略過：{item.PostId}");
                            _db.LinkPostToTask(task.Id, item.PostId); // 仍記錄任務關聯
                            continue;
                        }

                        // 爬取貼文內容頁
                        string postUrl  = ToAbsolute(item.Href);
                        AppLogger.Debug($"爬取貼文：{item.Title}（{item.PostId}）");
                        string postHtml = await FetchAsync(postUrl, ct);

                        var post = PttHtmlParser.ParsePostContent(postHtml, item.PostId, postUrl);
                        _db.InsertPost(post);
                        _db.LinkPostToTask(task.Id, item.PostId);
                        newPosts++;
                        AppLogger.Info($"新增貼文：{post.Title}（作者：{post.AuthorId}）");

                        await DelayAsync(ct);
                    }

                    pagesDone++;

                    // 頁數上限
                    if (task.MaxPages.HasValue && pagesDone >= task.MaxPages.Value)
                    {
                        AppLogger.Info($"已達頁數上限（{task.MaxPages.Value} 頁），停止爬取。");
                        break;
                    }

                    // 上頁
                    var prevRel = PttHtmlParser.GetPrevPageUrl(html);
                    if (prevRel == null)
                    {
                        AppLogger.Info("已到達最舊頁，爬取結束。");
                        break;
                    }
                    currentUrl = ToAbsolute(prevRel);

                    await DelayAsync(ct);
                }

                AppLogger.Info($"爬蟲任務完成。新增 {newPosts} 篇，略過 {skipped} 篇。");
            }
            catch (OperationCanceledException)
            {
                AppLogger.Info($"爬蟲任務已取消。新增 {newPosts} 篇，略過 {skipped} 篇。");
                throw;
            }
            catch (Exception ex)
            {
                AppLogger.Error("爬蟲任務執行時發生錯誤", ex);
                throw;
            }
            finally
            {
                _db.FinishExecution(execId, newPosts, skipped);
            }
        }

        // ── 輔助 ──────────────────────────────────────────────

        private static string BuildStartUrl(CrawlTask task)
        {
            if (!string.IsNullOrWhiteSpace(task.Keyword))
            {
                // 從看版 URL 推算 search URL
                // e.g. https://www.ptt.cc/bbs/AllTogether/index.html
                //   → https://www.ptt.cc/bbs/AllTogether/search?q=keyword
                var baseBoard = task.BoardUrl.TrimEnd('/');
                var idxIdx    = baseBoard.LastIndexOf('/');
                var boardBase = idxIdx >= 0 ? baseBoard[..idxIdx] : baseBoard;
                return $"{boardBase}/search?q={Uri.EscapeDataString(task.Keyword)}";
            }
            return task.BoardUrl;
        }

        private async Task<string> FetchAsync(string url, CancellationToken ct)
        {
            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var resp = await _http.GetAsync(url, ct);
                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadAsStringAsync(ct);
                }
                catch (HttpRequestException ex) when (attempt < maxRetries)
                {
                    AppLogger.Debug($"第 {attempt} 次請求失敗（{ex.Message}），稍後重試…");
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
                }
            }
            // 最後一次，讓例外往上傳
            using var finalResp = await _http.GetAsync(url, ct);
            finalResp.EnsureSuccessStatusCode();
            return await finalResp.Content.ReadAsStringAsync(ct);
        }

        private static string ToAbsolute(string href)
        {
            if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return href;
            return "https://www.ptt.cc" + href;
        }

        private static async Task DelayAsync(CancellationToken ct)
        {
            int ms = _rng.Next(500, 1001);
            await Task.Delay(ms, ct);
        }
    }
}
