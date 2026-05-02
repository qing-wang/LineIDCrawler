using LineIDAnalyzer.Models;

namespace LineIDAnalyzer.Business
{
    /// <summary>
    /// 批次執行測試案例，驗證 LineIDAnalyzer 的分析正確性。
    /// 從 testcases/lineid 與 testcases/nolineid 目錄讀取 .txt 測試檔。
    /// </summary>
    public class TestRunner
    {
        private readonly AppSettings _settings;

        /// <summary>testcases 根目錄（預設為執行目錄下的 testcases）。</summary>
        public string TestCasesRoot { get; set; }

        public TestRunner(AppSettings settings, string? testCasesRoot = null)
        {
            _settings      = settings;
            TestCasesRoot  = testCasesRoot
                ?? Path.Combine(AppContext.BaseDirectory, "testcases");
        }

        // ── 公開 API ──────────────────────────────────────────

        /// <summary>
        /// 載入所有測試案例（不執行分析）。
        /// </summary>
        public List<TestCase> LoadTestCases()
        {
            var cases = new List<TestCase>();
            cases.AddRange(LoadFromDirectory(Path.Combine(TestCasesRoot, "lineid"),   expectsLineId: true));
            cases.AddRange(LoadFromDirectory(Path.Combine(TestCasesRoot, "nolineid"), expectsLineId: false));
            return cases;
        }

        /// <summary>
        /// 循序執行所有測試案例，回傳彙總結果。
        /// </summary>
        /// <param name="progress">進度回呼（可為 null）。</param>
        /// <param name="cancellationToken">可取消 Token。</param>
        public async Task<TestRunSummary> RunAllAsync(
            IProgress<TestProgress>? progress     = null,
            CancellationToken        cancellationToken = default)
        {
            var cases    = LoadTestCases();
            var summary  = new TestRunSummary { StartedAt = DateTime.Now };
            var analyzer = new LineIDAnalyzer(_settings);
            var sw       = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < cases.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tc = cases[i];
                progress?.Report(new TestProgress
                {
                    Current         = i + 1,
                    Total           = cases.Count,
                    CurrentFileName = tc.FileName
                });

                var result = await RunSingleAsync(analyzer, tc, cancellationToken);
                summary.Results.Add(result);
            }

            sw.Stop();
            summary.TotalDuration = sw.Elapsed;
            return summary;
        }

        // ── 內部方法 ──────────────────────────────────────────

        private static List<TestCase> LoadFromDirectory(string dir, bool expectsLineId)
        {
            if (!Directory.Exists(dir))
                return new List<TestCase>();

            return Directory
                .GetFiles(dir, "*.txt", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f)
                .Select(path => new TestCase
                {
                    FilePath      = path,
                    Content       = File.ReadAllText(path, System.Text.Encoding.UTF8),
                    ExpectsLineId = expectsLineId
                })
                .ToList();
        }

        private static async Task<TestResult> RunSingleAsync(
            LineIDAnalyzer       analyzer,
            TestCase             tc,
            CancellationToken    cancellationToken)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Stage 1
            var keywordFound = LineIDAnalyzer.ContainsLineKeyword(tc.Content);
            var stage1Result = keywordFound ? Stage1Result.KeywordFound : Stage1Result.KeywordNotFound;

            // 若 Stage 1 過濾且預期有 ID → 直接判失敗（Stage 1 誤判）
            if (!keywordFound && tc.ExpectsLineId)
            {
                sw.Stop();
                return new TestResult
                {
                    TestCase      = tc,
                    Stage1Result  = stage1Result,
                    Passed        = false,
                    FailureReason = "Stage 1 誤判：未偵測到 LINE 關鍵字，但此案例應含有 Line ID",
                    Duration      = sw.Elapsed
                };
            }

            // 若 Stage 1 過濾且預期無 ID → 正確通過（無需呼叫 LLM）
            if (!keywordFound && !tc.ExpectsLineId)
            {
                sw.Stop();
                return new TestResult
                {
                    TestCase      = tc,
                    Stage1Result  = stage1Result,
                    AnalysisResult = new AnalysisResult { HasLineId = false },
                    Passed        = true,
                    Duration      = sw.Elapsed
                };
            }

            // Stage 2：LLM 分析
            AnalysisResult analysisResult;
            try
            {
                analysisResult = await analyzer.AnalyzeAsync(tc.Content, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw; // 由上層處理取消
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new TestResult
                {
                    TestCase      = tc,
                    Stage1Result  = stage1Result,
                    Passed        = false,
                    FailureReason = $"分析時發生例外：{ex.Message}",
                    Duration      = sw.Elapsed
                };
            }

            sw.Stop();

            // 判斷通過 / 失敗
            bool passed;
            string? failureReason = null;

            if (!analysisResult.IsSuccess)
            {
                passed        = false;
                failureReason = $"LLM 回傳錯誤：{analysisResult.ErrorMessage}";
            }
            else if (tc.ExpectsLineId && !analysisResult.HasLineId)
            {
                passed        = false;
                failureReason = "預期含有 Line ID，但 LLM 未偵測到";
            }
            else if (!tc.ExpectsLineId && analysisResult.HasLineId)
            {
                passed        = false;
                failureReason = $"預期不含 Line ID，但 LLM 誤報了：{string.Join(", ", analysisResult.ExtractedIds)}";
            }
            else
            {
                passed = true;
            }

            return new TestResult
            {
                TestCase       = tc,
                Stage1Result   = stage1Result,
                AnalysisResult = analysisResult,
                Passed         = passed,
                FailureReason  = failureReason,
                Duration       = sw.Elapsed
            };
        }
    }
}
