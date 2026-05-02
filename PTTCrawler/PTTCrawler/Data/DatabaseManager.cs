using Microsoft.Data.Sqlite;
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
                );";
            cmd.ExecuteNonQuery();
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
