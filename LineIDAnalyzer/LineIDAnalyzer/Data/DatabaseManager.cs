using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LineIDAnalyzer.Models;

namespace LineIDAnalyzer.Data
{
    /// <summary>
    /// 封裝所有 SQLite 資料庫存取操作。
    /// API Key 以 Windows DPAPI（ProtectedData）加密後儲存。
    /// </summary>
    public class DatabaseManager : IDisposable
    {
        private readonly string _connectionString;
        private bool _disposed;

        public DatabaseManager(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        // ── 初始化 ────────────────────────────────────────────

        private void InitializeDatabase()
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS app_settings (
                    key   TEXT PRIMARY KEY,
                    value TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS analysis_history (
                    id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    analyzed_at   TEXT    NOT NULL,
                    input_text    TEXT    NOT NULL,
                    has_line_id   INTEGER NOT NULL,
                    extracted_ids TEXT,
                    raw_response  TEXT
                );

                CREATE TABLE IF NOT EXISTS profile_history (
                    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    analyzed_at         TEXT    NOT NULL,
                    input_title         TEXT,
                    input_author_id     TEXT,
                    input_nickname      TEXT,
                    input_body          TEXT    NOT NULL,
                    gender              TEXT,
                    gender_source       TEXT,
                    age                 TEXT,
                    age_source          TEXT,
                    residential_area    TEXT,
                    area_source         TEXT,
                    interests           TEXT,
                    interests_source    TEXT,
                    relationship_status TEXT,
                    relationship_source TEXT,
                    occupation          TEXT,
                    occupation_source   TEXT,
                    raw_response        TEXT
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // ── 設定（app_settings）────────────────────────────────

        /// <summary>儲存設定值（字串，直接存）。</summary>
        public void SaveSetting(string key, string value)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO app_settings (key, value) VALUES ($key, $value)
                ON CONFLICT(key) DO UPDATE SET value = excluded.value;
            ";
            cmd.Parameters.AddWithValue("$key",   key);
            cmd.Parameters.AddWithValue("$value", value);
            cmd.ExecuteNonQuery();
        }

        /// <summary>讀取設定值（字串）。找不到時回傳 null。</summary>
        public string? GetSetting(string key)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT value FROM app_settings WHERE key = $key;";
            cmd.Parameters.AddWithValue("$key", key);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : null;
        }

        /// <summary>以 DPAPI 加密後儲存 API Key。</summary>
        public void SaveApiKey(string plainTextApiKey)
        {
            var encrypted = EncryptString(plainTextApiKey);
            SaveSetting("api_key", encrypted);
        }

        /// <summary>讀取並解密 API Key。找不到時回傳 null。</summary>
        public string? LoadApiKey()
        {
            var encrypted = GetSetting("api_key");
            return encrypted == null ? null : DecryptString(encrypted);
        }

        // ── 分析歷史（analysis_history）───────────────────────

        /// <summary>儲存一筆分析歷史記錄。</summary>
        public void SaveAnalysisHistory(string inputText, AnalysisResult result)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO analysis_history
                    (analyzed_at, input_text, has_line_id, extracted_ids, raw_response)
                VALUES ($at, $input, $hasId, $ids, $raw);
            ";
            cmd.Parameters.AddWithValue("$at",    DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("$input", inputText);
            cmd.Parameters.AddWithValue("$hasId", result.HasLineId ? 1 : 0);
            cmd.Parameters.AddWithValue("$ids",   JsonSerializer.Serialize(result.ExtractedIds));
            cmd.Parameters.AddWithValue("$raw",   result.RawResponse);
            cmd.ExecuteNonQuery();
        }

        /// <summary>取得最近 N 筆分析歷史。</summary>
        public List<(DateTime AnalyzedAt, string InputText, bool HasLineId, List<string> ExtractedIds)>
            GetRecentHistory(int limit = 50)
        {
            var list = new List<(DateTime, string, bool, List<string>)>();

            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT analyzed_at, input_text, has_line_id, extracted_ids
                FROM analysis_history
                ORDER BY id DESC
                LIMIT $limit;
            ";
            cmd.Parameters.AddWithValue("$limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var at    = DateTime.Parse(reader.GetString(0));
                var text  = reader.GetString(1);
                var hasId = reader.GetInt32(2) == 1;
                var ids   = JsonSerializer.Deserialize<List<string>>(reader.GetString(3)) ?? new();
                list.Add((at, text, hasId, ids));
            }

            return list;
        }

        // ── 人物分析歷史（profile_history）──────────────────────

        /// <summary>儲存一筆人物分析歷史記錄。</summary>
        public void SaveProfileHistory(AuthorProfileRequest request, AuthorProfile profile)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO profile_history (
                    analyzed_at, input_title, input_author_id, input_nickname, input_body,
                    gender, gender_source, age, age_source,
                    residential_area, area_source,
                    interests, interests_source,
                    relationship_status, relationship_source,
                    occupation, occupation_source, raw_response)
                VALUES (
                    $at, $title, $authorId, $nick, $body,
                    $gender, $genderSrc, $age, $ageSrc,
                    $area, $areaSrc,
                    $interests, $intSrc,
                    $rel, $relSrc,
                    $occ, $occSrc, $raw);";
            cmd.Parameters.AddWithValue("$at",       DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("$title",    (object?)request.Title    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$authorId", (object?)request.AuthorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$nick",     (object?)request.Nickname ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$body",     request.Body);
            cmd.Parameters.AddWithValue("$gender",   (object?)profile.Gender.Value             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$genderSrc",profile.Gender.Source.ToString());
            cmd.Parameters.AddWithValue("$age",      (object?)profile.Age.Value                ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$ageSrc",   profile.Age.Source.ToString());
            cmd.Parameters.AddWithValue("$area",     (object?)profile.ResidentialArea.Value    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$areaSrc",  profile.ResidentialArea.Source.ToString());
            cmd.Parameters.AddWithValue("$interests",(object?)profile.Interests.Value          ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$intSrc",   profile.Interests.Source.ToString());
            cmd.Parameters.AddWithValue("$rel",      (object?)profile.RelationshipStatus.Value ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$relSrc",   profile.RelationshipStatus.Source.ToString());
            cmd.Parameters.AddWithValue("$occ",      (object?)profile.Occupation.Value         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$occSrc",   profile.Occupation.Source.ToString());
            cmd.Parameters.AddWithValue("$raw",      profile.RawResponse);
            cmd.ExecuteNonQuery();
        }

        // ── 輔助方法 ──────────────────────────────────────────

        private SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>使用 Windows DPAPI 加密字串（僅限當前使用者）。</summary>
        private static string EncryptString(string plainText)
        {
            var bytes     = Encoding.UTF8.GetBytes(plainText);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>使用 Windows DPAPI 解密字串。</summary>
        private static string DecryptString(string cipherText)
        {
            var bytes     = Convert.FromBase64String(cipherText);
            var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                SqliteConnection.ClearAllPools();
            }
        }
    }
}