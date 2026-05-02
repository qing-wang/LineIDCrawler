namespace LineIDAnalyzer.Models
{
    /// <summary>單一測試案例（對應一個 .txt 檔案）。</summary>
    public class TestCase
    {
        /// <summary>檔案完整路徑。</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>僅檔案名稱（含副檔名）。</summary>
        public string FileName => Path.GetFileName(FilePath);

        /// <summary>檔案內容（待分析文字）。</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>預期此案例含有 Line ID。</summary>
        public bool ExpectsLineId { get; set; }
    }

    /// <summary>Stage 1 的判斷結果。</summary>
    public enum Stage1Result
    {
        /// <summary>偵測到 LINE 關鍵字，進入 LLM。</summary>
        KeywordFound,
        /// <summary>未偵測到 LINE 關鍵字，直接略過 LLM。</summary>
        KeywordNotFound
    }

    /// <summary>單一測試案例的執行結果。</summary>
    public class TestResult
    {
        public TestCase TestCase { get; set; } = new();

        /// <summary>Stage 1 關鍵字判斷結果。</summary>
        public Stage1Result Stage1Result { get; set; }

        /// <summary>LLM 分析結果（若 Stage 1 略過則為 null）。</summary>
        public AnalysisResult? AnalysisResult { get; set; }

        /// <summary>測試是否通過。</summary>
        public bool Passed { get; set; }

        /// <summary>失敗原因描述（通過時為 null）。</summary>
        public string? FailureReason { get; set; }

        /// <summary>本案例分析耗時。</summary>
        public TimeSpan Duration { get; set; }

        /// <summary>萃取到的 Line ID 清單（無則為空）。</summary>
        public IReadOnlyList<string> ExtractedIds =>
            AnalysisResult?.ExtractedIds ?? new List<string>();
    }

    /// <summary>整批測試執行的彙總結果。</summary>
    public class TestRunSummary
    {
        public List<TestResult> Results { get; set; } = new();
        public DateTime StartedAt { get; set; }
        public TimeSpan TotalDuration { get; set; }

        public int Total   => Results.Count;
        public int Passed  => Results.Count(r => r.Passed);
        public int Failed  => Results.Count(r => !r.Passed);

        /// <summary>Stage 1 誤判（lineid 資料夾卻未偵測到關鍵字）的案例數。</summary>
        public int Stage1FalseNegatives =>
            Results.Count(r => r.TestCase.ExpectsLineId
                            && r.Stage1Result == Stage1Result.KeywordNotFound);
    }

    /// <summary>測試執行進度（供 IProgress 回呼使用）。</summary>
    public class TestProgress
    {
        public int Current { get; set; }
        public int Total   { get; set; }
        public string CurrentFileName { get; set; } = string.Empty;
    }
}
