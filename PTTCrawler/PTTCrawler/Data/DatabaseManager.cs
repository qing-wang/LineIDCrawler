using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using PTTCrawler.Models;

namespace PTTCrawler.Data
{
    public class DatabaseManager : IDisposable
    {
        private readonly string _dbPath;
        private bool _disposed;

        public DatabaseManager()
        {
            var dir   = AppContext.BaseDirectory;
            _dbPath   = Path.Combine(dir, "pttcrawler.db");
            InitializeDatabase();
        }

        // ── 連線 ─────────────────────────────────────────────
        private SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            return conn;
        }

        private void InitializeDatabase()
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS crawl_tasks (
                    id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    name       TEXT NOT NULL,
                    task_type  TEXT NOT NULL DEFAULT 'CollectLineId',
                    board_url  TEXT NOT NULL,
                    keyword    TEXT,
                    max_pages  INTEGER,
                    status     TEXT NOT NULL DEFAULT 'Active',
                    created_at TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS posts (
                    id          TEXT PRIMARY KEY,
                    author_id   TEXT,
                    author_nick TEXT,
                    board       TEXT,
                    title       TEXT,
                    post_time   TEXT,
                    content     TEXT,
                    crawled_at  TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS crawl_executions (
                    id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    task_id         INTEGER NOT NULL,
                    started_at      TEXT NOT NULL,
                    finished_at     TEXT,
                    new_post_count  INTEGER DEFAULT 0,
                    skipped_count   INTEGER DEFAULT 0,
                    FOREIGN KEY (task_id) REFERENCES crawl_tasks(id)
                );
                CREATE TABLE IF NOT EXISTS task_posts (
                    task_id  INTEGER NOT NULL,
                    post_id  TEXT    NOT NULL,
                    PRIMARY KEY (task_id, post_id),
                    FOREIGN KEY (task_id) REFERENCES crawl_tasks(id),
                    FOREIGN KEY (post_id) REFERENCES posts(id)
                );
                CREATE TABLE IF NOT EXISTS app_settings (
                    key   TEXT PRIMARY KEY,
                    value TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS profile_history (
                    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    analyzed_at         TEXT    NOT NULL,
                    post_id             TEXT,
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
                );";
            cmd.ExecuteNonQuery();

            BackfillTaskPosts(conn);
        }

        /// <summary>
        /// 補連舊有資料：依任務的 board_url 推算看版名稱，
        /// 將該看版下所有貼文以 INSERT OR IGNORE 補寫入 task_posts。
        /// 此方法冪等，每次啟動執行亦無害。
        /// </summary>
        private static void BackfillTaskPosts(SqliteConnection conn)
        {
            // 取得所有任務的 id + board_url
            var tasks = new List<(int Id, string BoardUrl)>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, board_url FROM crawl_tasks;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    tasks.Add(((int)(long)reader[0], reader[1].ToString()!));
            }

            foreach (var (taskId, boardUrl) in tasks)
            {
                var board = ExtractBoardFromUrl(boardUrl);
                if (string.IsNullOrEmpty(board)) continue;

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO task_posts (task_id, post_id)
                    SELECT @tid, id FROM posts WHERE board = @board;";
                cmd.Parameters.AddWithValue("@tid",   taskId);
                cmd.Parameters.AddWithValue("@board", board);
                cmd.ExecuteNonQuery();
            }
        }

        // 從 board_url 萃取看版名稱，例：
        //   https://www.ptt.cc/bbs/AllTogether/index.html → AllTogether
        private static string ExtractBoardFromUrl(string url)
        {
            var m = System.Text.RegularExpressions.Regex.Match(url, @"/bbs/([^/?#]+)");
            return m.Success ? m.Groups[1].Value : string.Empty;
        }

        // ── CrawlTask CRUD ────────────────────────────────────

        public List<CrawlTask> GetAllTasks()
        {
            var list = new List<CrawlTask>();
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT id,name,task_type,board_url,keyword,max_pages,status,created_at FROM crawl_tasks ORDER BY id DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapTask(reader));
            return list;
        }

        public CrawlTask? GetTask(int id)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT id,name,task_type,board_url,keyword,max_pages,status,created_at FROM crawl_tasks WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapTask(reader) : null;
        }

        public int InsertTask(CrawlTask task)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO crawl_tasks (name,task_type,board_url,keyword,max_pages,status,created_at)
                VALUES (@name,@type,@url,@kw,@mp,@status,@ca);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@name",   task.Name);
            cmd.Parameters.AddWithValue("@type",   task.TaskType.ToString());
            cmd.Parameters.AddWithValue("@url",    task.BoardUrl);
            cmd.Parameters.AddWithValue("@kw",     (object?)task.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mp",     (object?)task.MaxPages ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", task.Status);
            cmd.Parameters.AddWithValue("@ca",     task.CreatedAt.ToString("o"));
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void UpdateTask(CrawlTask task)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE crawl_tasks SET
                    name=@name, task_type=@type, board_url=@url,
                    keyword=@kw, max_pages=@mp, status=@status
                WHERE id=@id;";
            cmd.Parameters.AddWithValue("@name",   task.Name);
            cmd.Parameters.AddWithValue("@type",   task.TaskType.ToString());
            cmd.Parameters.AddWithValue("@url",    task.BoardUrl);
            cmd.Parameters.AddWithValue("@kw",     (object?)task.Keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mp",     (object?)task.MaxPages ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", task.Status);
            cmd.Parameters.AddWithValue("@id",     task.Id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteTask(int id)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM crawl_executions WHERE task_id=@id; DELETE FROM crawl_tasks WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void AbandonTask(int id)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "UPDATE crawl_tasks SET status='Abandoned' WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ── Post ──────────────────────────────────────────────

        public bool PostExists(string postId)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM posts WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", postId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void InsertPost(Post post)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR IGNORE INTO posts (id,author_id,author_nick,board,title,post_time,content,crawled_at)
                VALUES (@id,@aid,@anick,@board,@title,@ptime,@content,@ca);";
            cmd.Parameters.AddWithValue("@id",      post.Id);
            cmd.Parameters.AddWithValue("@aid",     (object?)post.AuthorId   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@anick",   (object?)post.AuthorNick ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@board",   (object?)post.Board      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@title",   (object?)post.Title      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ptime",   (object?)post.PostTime   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@content", (object?)post.Content    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ca",      post.CrawledAt.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        // ── CrawlExecution ────────────────────────────────────

        public int InsertExecution(int taskId)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO crawl_executions (task_id,started_at,new_post_count,skipped_count)
                VALUES (@tid,@sa,0,0);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@sa",  DateTime.Now.ToString("o"));
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void FinishExecution(int execId, int newPosts, int skipped)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE crawl_executions
                SET finished_at=@fa, new_post_count=@np, skipped_count=@sc
                WHERE id=@id;";
            cmd.Parameters.AddWithValue("@fa", DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("@np", newPosts);
            cmd.Parameters.AddWithValue("@sc", skipped);
            cmd.Parameters.AddWithValue("@id", execId);
            cmd.ExecuteNonQuery();
        }

        private static CrawlTask MapTask(SqliteDataReader r)
        {
            Enum.TryParse<CrawlTaskType>(r.GetString(2), out var tt);
            return new CrawlTask
            {
                Id        = r.GetInt32(0),
                Name      = r.GetString(1),
                TaskType  = tt,
                BoardUrl  = r.GetString(3),
                Keyword   = r.IsDBNull(4) ? null : r.GetString(4),
                MaxPages  = r.IsDBNull(5) ? null : r.GetInt32(5),
                Status    = r.GetString(6),
                CreatedAt = DateTime.Parse(r.GetString(7))
            };
        }

        private static Post MapPost(SqliteDataReader r)
        {
            return new Post
            {
                Id          = r.GetString(0),
                AuthorId    = r.IsDBNull(1) ? null : r.GetString(1),
                AuthorNick  = r.IsDBNull(2) ? null : r.GetString(2),
                Board       = r.IsDBNull(3) ? null : r.GetString(3),
                Title       = r.IsDBNull(4) ? null : r.GetString(4),
                PostTime    = r.IsDBNull(5) ? null : r.GetString(5),
                Content     = r.IsDBNull(6) ? null : r.GetString(6),
                CrawledAt   = DateTime.Parse(r.GetString(7))
            };
        }

        // ── task_posts ────────────────────────────────────────

        public void LinkPostToTask(int taskId, string postId)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO task_posts (task_id, post_id) VALUES (@tid, @pid);";
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@pid", postId);
            cmd.ExecuteNonQuery();
        }

        // ── 貼文瀏覽查詢 ──────────────────────────────────────

        public List<string> GetDistinctBoards()
        {
            var list = new List<string>();
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT board FROM posts WHERE board IS NOT NULL ORDER BY board;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(reader.GetString(0));
            return list;
        }

        public int GetPostCountByBoard(string board)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM posts WHERE board=@board;";
            cmd.Parameters.AddWithValue("@board", board);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Post> GetPostsByBoard(string board, int page, int pageSize, bool ascending)
        {
            var list  = new List<Post>();
            var order = ascending ? "ASC" : "DESC";
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT id,author_id,author_nick,board,title,post_time,content,crawled_at
                FROM posts WHERE board=@board
                ORDER BY COALESCE(post_time, crawled_at) {order}
                LIMIT @size OFFSET @offset;";
            cmd.Parameters.AddWithValue("@board",  board);
            cmd.Parameters.AddWithValue("@size",   pageSize);
            cmd.Parameters.AddWithValue("@offset", page * pageSize);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapPost(reader));
            return list;
        }

        public List<CrawlTask> GetTasksWithPosts()
        {
            var list = new List<CrawlTask>();
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT t.id,t.name,t.task_type,t.board_url,t.keyword,t.max_pages,t.status,t.created_at
                FROM crawl_tasks t
                INNER JOIN task_posts tp ON tp.task_id = t.id
                ORDER BY t.id DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapTask(reader));
            return list;
        }

        public int GetPostCountByTask(int taskId)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM task_posts WHERE task_id=@tid;";
            cmd.Parameters.AddWithValue("@tid", taskId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Post> GetPostsByTask(int taskId, int page, int pageSize, bool ascending)
        {
            var list  = new List<Post>();
            var order = ascending ? "ASC" : "DESC";
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT p.id,p.author_id,p.author_nick,p.board,p.title,p.post_time,p.content,p.crawled_at
                FROM posts p
                INNER JOIN task_posts tp ON tp.post_id = p.id
                WHERE tp.task_id=@tid
                ORDER BY COALESCE(p.post_time, p.crawled_at) {order}
                LIMIT @size OFFSET @offset;";
            cmd.Parameters.AddWithValue("@tid",    taskId);
            cmd.Parameters.AddWithValue("@size",   pageSize);
            cmd.Parameters.AddWithValue("@offset", page * pageSize);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapPost(reader));
            return list;
        }

        public Post? GetPost(string id)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT id,author_id,author_nick,board,title,post_time,content,crawled_at FROM posts WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapPost(reader) : null;
        }

        // ── app_settings / API Key ────────────────────────────

        public void SaveSetting(string key, string value)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO app_settings (key, value) VALUES (@key, @value)
                ON CONFLICT(key) DO UPDATE SET value = excluded.value;";
            cmd.Parameters.AddWithValue("@key",   key);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.ExecuteNonQuery();
        }

        public string? GetSetting(string key)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT value FROM app_settings WHERE key=@key;";
            cmd.Parameters.AddWithValue("@key", key);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : null;
        }

        public void SaveApiKey(string plainText)
        {
            var encrypted = EncryptString(plainText);
            SaveSetting("api_key", encrypted);
        }

        public string? LoadApiKey()
        {
            var encrypted = GetSetting("api_key");
            return encrypted == null ? null : TryDecryptString(encrypted);
        }

        public AppSettings LoadAppSettings()
        {
            return new AppSettings
            {
                ApiKey         = LoadApiKey() ?? string.Empty,
                ModelName      = GetSetting("model_name")      ?? "gpt-4o-mini",
                TimeoutSeconds = int.TryParse(GetSetting("timeout_seconds"), out var t) ? t : 60
            };
        }

        public void SaveAppSettings(AppSettings s)
        {
            if (!string.IsNullOrWhiteSpace(s.ApiKey)) SaveApiKey(s.ApiKey);
            SaveSetting("model_name",      s.ModelName);
            SaveSetting("timeout_seconds", s.TimeoutSeconds.ToString());
        }

        private static string EncryptString(string plain)
        {
            var bytes = Encoding.UTF8.GetBytes(plain);
            var enc   = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(enc);
        }

        private static string? TryDecryptString(string cipher)
        {
            try
            {
                var bytes = Convert.FromBase64String(cipher);
                var dec   = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(dec);
            }
            catch { return null; }
        }

        // ── 分析用：全量貼文查詢（不分頁）────────────────────

        public List<Post> GetAllPostsByBoard(string board, bool ascending)
        {
            var list  = new List<Post>();
            var order = ascending ? "ASC" : "DESC";
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT id,author_id,author_nick,board,title,post_time,content,crawled_at
                FROM posts WHERE board=@board
                ORDER BY COALESCE(post_time, crawled_at) {order};";
            cmd.Parameters.AddWithValue("@board", board);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapPost(reader));
            return list;
        }

        public List<Post> GetAllPostsByTask(int taskId, bool ascending)
        {
            var list  = new List<Post>();
            var order = ascending ? "ASC" : "DESC";
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT p.id,p.author_id,p.author_nick,p.board,p.title,p.post_time,p.content,p.crawled_at
                FROM posts p
                INNER JOIN task_posts tp ON tp.post_id = p.id
                WHERE tp.task_id=@tid
                ORDER BY COALESCE(p.post_time, p.crawled_at) {order};";
            cmd.Parameters.AddWithValue("@tid", taskId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapPost(reader));
            return list;
        }

        // ── 人物分析歷史（profile_history）──────────────────────

        /// <summary>儲存一筆人物分析歷史記錄。</summary>
        public void SaveProfileHistory(string? postId, AuthorProfileRequest request, AuthorProfile profile)
        {
            using var conn = OpenConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO profile_history (
                    analyzed_at, post_id, input_title, input_author_id, input_nickname, input_body,
                    gender, gender_source, age, age_source,
                    residential_area, area_source,
                    interests, interests_source,
                    relationship_status, relationship_source,
                    occupation, occupation_source, raw_response)
                VALUES (
                    $at, $postId, $title, $authorId, $nick, $body,
                    $gender, $genderSrc, $age, $ageSrc,
                    $area, $areaSrc,
                    $interests, $intSrc,
                    $rel, $relSrc,
                    $occ, $occSrc, $raw);";
            cmd.Parameters.AddWithValue("$at",       DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("$postId",   (object?)postId           ?? DBNull.Value);
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
