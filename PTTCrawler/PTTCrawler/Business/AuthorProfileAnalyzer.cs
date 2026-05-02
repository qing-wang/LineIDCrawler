using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using PTTCrawler.Models;

namespace PTTCrawler.Business
{
    public class AuthorProfileAnalyzer
    {
        private const string SystemPrompt = @"
你是一個專門分析 PTT 貼文作者個人資料的助手。根據提供的貼文資訊（可能包含標題、作者 PTT ID、暱稱、內文），分析以下屬性：
1. 性別（gender）：""男性"" / ""女性"" / null
2. 年紀（age）：具體數字或描述（如 ""28""、""30多歲""）/ null
3. 居住地區（residentialArea）：城市或地區（如 ""台北""、""高雄""）/ null
4. 興趣（interests）：興趣描述（如 ""運動、音樂""）/ null
5. 感情狀態（relationshipStatus）：""單身"" / ""交往中"" / ""已婚"" / ""其他"" / null
6. 職業/身份（occupation）：""上班族"" / ""學生"" / ""其他（含具體說明）"" / null

每個欄位的 source（資料來源）：
- ""自陳""：作者在文章中明確陳述
- ""推斷""：根據語氣、用詞、語境推斷
- ""無法分析""：資訊不足（此時 value 必須為 null）

Few-Shot 範例：
輸入：[內文]：我 28 歲男生住台北，喜歡打籃球，目前單身，在科技業上班，想認識女生。
輸出：{""gender"":{""value"":""男性"",""source"":""自陳""},""age"":{""value"":""28"",""source"":""自陳""},""residentialArea"":{""value"":""台北"",""source"":""自陳""},""interests"":{""value"":""籃球"",""source"":""自陳""},""relationshipStatus"":{""value"":""單身"",""source"":""自陳""},""occupation"":{""value"":""上班族（科技業）"",""source"":""自陳""}}

輸入：[內文]：今天下班後去看展，很累。最近工作壓力好大，希望能找個人陪。
輸出：{""gender"":{""value"":null,""source"":""無法分析""},""age"":{""value"":null,""source"":""無法分析""},""residentialArea"":{""value"":null,""source"":""無法分析""},""interests"":{""value"":""看展"",""source"":""推斷""},""relationshipStatus"":{""value"":""單身"",""source"":""推斷""},""occupation"":{""value"":""上班族"",""source"":""推斷""}}

輸入：[標題]：[尋緣] 25F 高雄
[暱稱]：小美
[內文]：喜歡追劇和手作，下班後都宅在家，想找一個穩定的人。
輸出：{""gender"":{""value"":""女性"",""source"":""自陳""},""age"":{""value"":""25"",""source"":""自陳""},""residentialArea"":{""value"":""高雄"",""source"":""自陳""},""interests"":{""value"":""追劇、手作"",""source"":""自陳""},""relationshipStatus"":{""value"":""單身"",""source"":""推斷""},""occupation"":{""value"":""上班族"",""source"":""推斷""}}

請嚴格以 JSON 格式回應，不要加任何解釋文字：
{
  ""gender"":             { ""value"": ""..."",  ""source"": ""..."" },
  ""age"":                { ""value"": ""..."",  ""source"": ""..."" },
  ""residentialArea"":    { ""value"": ""..."",  ""source"": ""..."" },
  ""interests"":          { ""value"": ""..."",  ""source"": ""..."" },
  ""relationshipStatus"": { ""value"": ""..."",  ""source"": ""..."" },
  ""occupation"":         { ""value"": ""..."",  ""source"": ""..."" }
}
";

        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly int    _timeoutSeconds;

        public AuthorProfileAnalyzer(AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
                throw new ArgumentException("API Key 不可為空白。", nameof(settings));

            _apiKey         = settings.ApiKey;
            _modelName      = settings.ModelName;
            _timeoutSeconds = settings.TimeoutSeconds;
        }

        public async Task<AuthorProfile> AnalyzeAsync(
            AuthorProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Body))
                return new AuthorProfile { ErrorMessage = "內文不可為空白。" };

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(request.Title))    sb.AppendLine($"[標題]：{request.Title}");
            if (!string.IsNullOrWhiteSpace(request.AuthorId)) sb.AppendLine($"[作者 PTT ID]：{request.AuthorId}");
            if (!string.IsNullOrWhiteSpace(request.Nickname)) sb.AppendLine($"[暱稱]：{request.Nickname}");
            sb.AppendLine($"[內文]：{request.Body}");

            string rawText = string.Empty;
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

                var client   = new ChatClient(_modelName, _apiKey);
                var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(SystemPrompt.Trim()),
                    ChatMessage.CreateUserMessage(sb.ToString())
                };

                var completion = await client.CompleteChatAsync(messages, cancellationToken: cts.Token);
                rawText = completion.Value.Content[0].Text;

                return ParseResponse(rawText);
            }
            catch (OperationCanceledException)
            {
                return new AuthorProfile { RawResponse = rawText, ErrorMessage = "分析已取消或逾時。" };
            }
            catch (Exception ex)
            {
                return new AuthorProfile { RawResponse = rawText, ErrorMessage = $"呼叫 LLM API 時發生錯誤：{ex.Message}" };
            }
        }

        private static AuthorProfile ParseResponse(string rawText)
        {
            try
            {
                var jsonStart = rawText.IndexOf('{');
                var jsonEnd   = rawText.LastIndexOf('}');
                if (jsonStart < 0 || jsonEnd < 0 || jsonEnd < jsonStart)
                    return new AuthorProfile { RawResponse = rawText, ErrorMessage = $"無法從回應中找到 JSON 區塊：{rawText}" };

                var json = rawText[jsonStart..(jsonEnd + 1)];
                using var doc  = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new AuthorProfile
                {
                    Gender             = ParseField(root, "gender"),
                    Age                = ParseField(root, "age"),
                    ResidentialArea    = ParseField(root, "residentialArea"),
                    Interests          = ParseField(root, "interests"),
                    RelationshipStatus = ParseField(root, "relationshipStatus"),
                    Occupation         = ParseField(root, "occupation"),
                    RawResponse        = rawText
                };
            }
            catch (Exception ex)
            {
                return new AuthorProfile { RawResponse = rawText, ErrorMessage = $"解析 LLM 回應時發生錯誤：{ex.Message}" };
            }
        }

        private static ProfileField ParseField(JsonElement root, string fieldName)
        {
            if (!root.TryGetProperty(fieldName, out var fieldElem))
                return new ProfileField();

            string? value = null;
            if (fieldElem.TryGetProperty("value", out var valueElem) &&
                valueElem.ValueKind != JsonValueKind.Null)
                value = valueElem.GetString();

            ProfileSource source = ProfileSource.無法分析;
            if (fieldElem.TryGetProperty("source", out var sourceElem))
            {
                source = sourceElem.GetString() switch
                {
                    "自陳" => ProfileSource.自陳,
                    "推斷" => ProfileSource.推斷,
                    _     => ProfileSource.無法分析
                };
            }

            if (string.IsNullOrWhiteSpace(value))
                source = ProfileSource.無法分析;

            return new ProfileField { Value = value, Source = source };
        }
    }
}
