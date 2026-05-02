using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.RegularExpressions;
using LineIDAnalyzer.Models;

namespace LineIDAnalyzer.Business
{
    /// <summary>
    /// 核心 Line ID 分析邏輯。
    /// 採用兩階段混合式演算法：
    ///   Stage 1 — 本地端關鍵字存在性判斷（無 API 費用）
    ///   Stage 2 — LLM 全文語意分析與萃取（有 API 費用）
    /// 此類別不依賴 WinForms，可獨立作為 Class Library 使用。
    /// </summary>
    public class LineIDAnalyzer
    {
        // ── Stage 1：關鍵字清單 ───────────────────────────────
        // 用於快速比對明確的複合詞組（例：line帳號、加我line）。
        private static readonly string[] LineKeywords =
        {
            "line id", "line i.d", "lineid",
            "line帳號", "line號碼", "line account",
            "加line", "加 line", "加我line", "加我 line",
            "我的line", "我的 line",
            "line是", "line：", "line:", "line的id", "line的帳號",
            "賴:", "賴：", "加賴", "加我賴", "我的賴", "賴帳號", "賴id",
        };

        // LINE 作為獨立詞的 Regex（不可是英文單字的一部分）
        // (?<![a-zA-Z]) 確保前面不是英文字母，(?![a-zA-Z]) 確保後面不是英文字母
        // 因此 "online" / "deadline" 不命中，但 "LINE我" / "可以LINE" / "LINE 給我" 都命中
        private static readonly Regex StandaloneLineRegex = new(
            @"(?<![a-zA-Z])line(?![a-zA-Z])",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // ── Stage 2：LLM System Prompt ────────────────────────
        private const string SystemPrompt = @"
你是一個專門分析文字中是否含有 LINE ID 的助手。

LINE ID 格式規則：
- 長度為 4 至 20 個字元
- 只能包含英文字母（a-z, A-Z）、數字（0-9）、底線（_）、連字符（-）、點（.）
- 不能以特殊符號（_ - .）開頭或結尾
- 不包含空白字元

判斷原則：
- 只有在文字「明確表示」某個字串是 LINE 聯絡帳號時，才認定為 LINE ID。
- 例如：「加我 line: abc123」、「我的賴是 john_doe」、「line帳號 john-123」。
- 即使字串符合格式，若文字沒有 LINE 相關脈絡，不可認定為 LINE ID。
- 關鍵字與 ID 之間可能有任意距離，請根據整篇文字的語意判斷。

Few-Shot 範例：
輸入：「有問題加我 LINE，帳號是 john_doe123，謝謝」
輸出：{""hasLineId"": true, ""extractedIds"": [""john_doe123""]}

輸入：「這個商品型號是 AB-1234，請參考規格表」
輸出：{""hasLineId"": false, ""extractedIds"": []}

輸入：「可以加我賴或 ig 都可以，line 的話 id 是 my.id_01」
輸出：{""hasLineId"": true, ""extractedIds"": [""my.id_01""]}

輸入：「我平常都用 LINE 比較多。有問題的話可以直接找我。帳號：cool_user」
輸出：{""hasLineId"": true, ""extractedIds"": [""cool_user""]}

輸入：「加我 line 或 ig 都行：line/john_doe、ig/john_doe_art」
輸出：{""hasLineId"": true, ""extractedIds"": [""john_doe""]}

輸入：「可以LINE我有真相 nexus0814」
輸出：{""hasLineId"": true, ""extractedIds"": [""nexus0814""]}

請嚴格以 JSON 格式回應，不要加上任何解釋文字：
{
  ""hasLineId"": true 或 false,
  ""extractedIds"": [""id1"", ""id2""]
}
";

        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly int    _timeoutSeconds;

        public LineIDAnalyzer(AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
                throw new ArgumentException("API Key 不可為空白。", nameof(settings));

            _apiKey         = settings.ApiKey;
            _modelName      = settings.ModelName;
            _timeoutSeconds = settings.TimeoutSeconds;
        }

        /// <summary>
        /// 分析輸入文字是否含有 Line ID。
        /// Stage 1：本地端關鍵字判斷；Stage 2：LLM 全文語意分析。
        /// </summary>
        /// <param name="inputText">待分析的文字。</param>
        /// <param name="cancellationToken">可取消操作的 Token。</param>
        /// <returns>分析結果。</returns>
        public async Task<AnalysisResult> AnalyzeAsync(
            string inputText,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return new AnalysisResult
                {
                    HasLineId    = false,
                    ErrorMessage = "輸入文字不可為空白。"
                };
            }

            // ── Stage 1：關鍵字存在性判斷 ────────────────────
            if (!ContainsLineKeyword(inputText))
            {
                return new AnalysisResult
                {
                    HasLineId   = false,
                    RawResponse = "[Stage 1] 未偵測到任何 LINE 相關關鍵字，略過 LLM 分析。"
                };
            }

            // ── Stage 2：LLM 全文語意分析 ─────────────────────
            return await CallLlmAsync(inputText, cancellationToken);
        }

        /// <summary>
        /// 測試 API Key 是否有效（送出最小化的請求）。
        /// </summary>
        public async Task<(bool Success, string Message)> TestConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client     = new OpenAIClient(_apiKey);
                var chatClient = client.GetChatClient(_modelName);

                var messages = new List<ChatMessage>
                {
                    new UserChatMessage("回覆「OK」")
                };

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(15));

                await chatClient.CompleteChatAsync(messages, cancellationToken: cts.Token);
                return (true, "連線成功！API Key 有效。");
            }
            catch (OperationCanceledException)
            {
                return (false, "連線逾時，請確認網路狀態。");
            }
            catch (Exception ex)
            {
                return (false, $"連線失敗：{ex.Message}");
            }
        }

        // ── Stage 1：關鍵字判斷 ───────────────────────────────

        /// <summary>
        /// 判斷文字中是否含有任何 LINE 相關訊號。
        /// 兩種判斷方式，任一命中即進入 Stage 2：
        ///   1. 複合詞組清單（例：line帳號、加我line）
        ///   2. LINE 作為獨立詞（例：LINE我、可以LINE、傳LINE給我）
        /// </summary>
        public static bool ContainsLineKeyword(string text)
        {
            var lower = text.ToLowerInvariant();

            // 方式一：複合詞組（快速 Contains）
            if (LineKeywords.Any(lower.Contains))
                return true;

            // 方式二：LINE 作為獨立詞（Regex，排除 online/deadline 等英文字詞）
            return StandaloneLineRegex.IsMatch(text);
        }

        // ── Stage 2：LLM 呼叫 ────────────────────────────────

        private async Task<AnalysisResult> CallLlmAsync(
            string inputText,
            CancellationToken cancellationToken)
        {
            try
            {
                var client     = new OpenAIClient(_apiKey);
                var chatClient = client.GetChatClient(_modelName);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(SystemPrompt),
                    new UserChatMessage($"請分析以下文字：\n\n{inputText}")
                };

                var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cts.Token);
                var rawText  = response.Value.Content[0].Text?.Trim() ?? string.Empty;

                return ParseLlmResponse(rawText);
            }
            catch (OperationCanceledException)
            {
                return new AnalysisResult { ErrorMessage = "分析已被取消。" };
            }
            catch (Exception ex)
            {
                return new AnalysisResult
                {
                    ErrorMessage = $"呼叫 API 時發生錯誤：{ex.Message}"
                };
            }
        }

        // ── 回應解析 ──────────────────────────────────────────

        private static AnalysisResult ParseLlmResponse(string rawText)
        {
            try
            {
                // 嘗試從回應中取出 JSON 區塊（LLM 有時會包在 ```json ... ``` 中）
                var json = ExtractJson(rawText);

                using var doc = JsonDocument.Parse(json);
                var root      = doc.RootElement;

                var hasLineId    = root.GetProperty("hasLineId").GetBoolean();
                var extractedIds = new List<string>();

                if (root.TryGetProperty("extractedIds", out var idsElem))
                {
                    foreach (var idElem in idsElem.EnumerateArray())
                    {
                        var id = idElem.GetString();
                        if (!string.IsNullOrWhiteSpace(id))
                            extractedIds.Add(id);
                    }
                }

                return new AnalysisResult
                {
                    HasLineId    = hasLineId,
                    ExtractedIds = extractedIds,
                    RawResponse  = rawText
                };
            }
            catch (Exception ex)
            {
                return new AnalysisResult
                {
                    RawResponse  = rawText,
                    ErrorMessage = $"解析 LLM 回應時發生錯誤：{ex.Message}\n原始回應：{rawText}"
                };
            }
        }

        private static string ExtractJson(string text)
        {
            // 去除 ```json ... ``` 或 ``` ... ``` 標記
            var start = text.IndexOf('{');
            var end   = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                return text[start..(end + 1)];
            return text;
        }
    }
}
