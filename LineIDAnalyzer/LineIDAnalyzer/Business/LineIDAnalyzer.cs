using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using LineIDAnalyzer.Models;

namespace LineIDAnalyzer.Business
{
    /// <summary>
    /// 核心 Line ID 分析邏輯。
    /// 此類別不依賴 WinForms，可獨立作為 Class Library 使用。
    /// </summary>
    public class LineIDAnalyzer
    {
        private const string SystemPrompt = @"
你是一個專門分析文字中是否含有 LINE ID 的助手。

LINE ID 的規則：
- 長度為 4 至 20 個字元
- 只能包含英文字母（a-z, A-Z）、數字（0-9）、底線（_）、連字符（-）、點（.）
- 不能以特殊符號開頭或結尾
- 不包含空白字元

請嚴格以 JSON 格式回應，不要加上任何解釋文字，格式如下：
{
  ""hasLineId"": true 或 false,
  ""extractedIds"": [""id1"", ""id2""]   // 若無 Line ID 則為空陣列 []
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

            try
            {
                var client = new OpenAIClient(_apiKey);
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

        // ── 私有方法 ──────────────────────────────────────────

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
