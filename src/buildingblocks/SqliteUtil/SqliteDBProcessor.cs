internal class SqliteDBProcessor : ISqliteDBProcessor
{
    static DbProviderFactory mDBFactory;
    protected List<string> EnsuredTypeList { get; set; }
    protected bool SplitTable { get; set; }
    protected string FileOrDirectoryPath { get; set; }

    protected List<string> InitedTableList { get; set; }

    static SqliteDBProcessor()
    {
        mDBFactory = new System.Data.SQLite.SQLiteFactory();
    }

    public static void SetDBFactory(DbProviderFactory dbProviderFactory)
    {
        mDBFactory = dbProviderFactory;
    }

    public static SqliteDBProcessor GetProcessor(string path, bool splittable = false)
    {
        return new SqliteDBProcessor($"{path}", splittable);
    }
    public SqliteDBProcessor(string connection, bool splittable)
    {
        FileOrDirectoryPath = connection;
        SplitTable = splittable;
        InitedTableList = new List<string>();
        EnsuredTypeList = new List<string>();
    }

    private DbConnection OpenConnection<T>()
    {
        lock (EnsuredTypeList)
        {
            var conn = mDBFactory.CreateConnection();
            conn.ConnectionString = GetConnectionString<T>();
            conn.Open();

            if (!EnsuredTypeList.Contains(typeof(T).FullName))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = SqliteUtil.GenerateCreateTableCommand<T>();
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
            return conn;
        }
    }

    private Task<DbConnection> OpenConnectionAsync<T>()
    {
        return Task.Run(() =>
        {
            lock (EnsuredTypeList)
            {
                var conn = mDBFactory.CreateConnection();
                conn.ConnectionString = GetConnectionString<T>();
                conn.Open();

                if (!EnsuredTypeList.Contains(typeof(T).FullName))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = SqliteUtil.GenerateCreateTableCommand<T>();
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
                return conn;
            }
        });
    }

    private string GenerateConnectionString(string path)
    {
        return $"Data Source={path}";
    }
    private string GetConnectionString<T>()
    {
        if (SplitTable)
        {
            FileInfo fileInfo = new FileInfo(FileOrDirectoryPath);
            DirectoryInfo directoryInfo;
            if (fileInfo.Exists)
            {
                directoryInfo = fileInfo.Directory;
                return GenerateConnectionString(FileOrDirectoryPath);
            }
            else
            {
                directoryInfo = new DirectoryInfo(FileOrDirectoryPath);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                return GenerateConnectionString(Path.Combine(directoryInfo.FullName, $"{typeof(T).Name}.db"));
            }

        }
        return GenerateConnectionString(FileOrDirectoryPath);
    }

    private void EnsureDataLocation<T>()
    {
        using (var conn = OpenConnection<T>())
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SqliteUtil.GenerateCreateTableCommand<T>();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
    public void CommitItems<T>(List<T> items)
    {
        ExecuteTransactionScope<T>((conn) =>
        {
            foreach (var item in items)
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqliteUtil.GenerateInsertCommand<T>();
                    command.CommandType = System.Data.CommandType.Text;
                    foreach (var parameter in SqliteUtil.GenerateInsertDatabaseParameters<T>(item))
                    {
                        var param = command.CreateParameter();
                        param.ParameterName = parameter.Key;
                        param.Value = parameter.Value;
                        command.Parameters.Add(param);
                    }
                    command.ExecuteNonQuery();

                }
            }
        });
    }


    public IEnumerable<T> ReadItems<T>()
    {
        using (var conn = OpenConnection<T>())
        {


            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SqliteUtil.GenerateReadCommand<T>();
                cmd.CommandType = System.Data.CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            yield return SqliteUtil.ReadObject<T>(reader);
                        }
                    }
                }
            }
        }
    }
#if NET5_0_OR_GREATER
    public async IAsyncEnumerable<T> ReadItemsAsync<T>()
    {
        using (var conn = OpenConnection<T>())
        {


            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SqliteUtil.GenerateReadCommand<T>();
                cmd.CommandType = System.Data.CommandType.Text;
                using (var reader = cmd.ExecuteReaderAsync())
                {

                    if ((await reader).HasRows)
                    {
                        while (await (await reader).ReadAsync())
                        {
                            yield return SqliteUtil.ReadObject<T>(await reader);
                        }
                    }
                }
            }
        }
    }
#endif
    private void ExecuteTransactionScope<T>(Action<DbConnection> action)
    {
        using (var conn = OpenConnection<T>())
        using (var traction = conn.BeginTransaction())
        {
            action(conn);
            traction.Commit();
        }
    }

    private async Task ExecuteTransactionScopeAsync<T>(Action<DbConnection> action)
    {
        using (var conn = await OpenConnectionAsync<T>())
        {
            await Task.Run(
                 () =>
                {
                    using (var traction = conn.BeginTransaction())
                    {
                        action.Invoke(conn);
                        traction.Commit();
                    }
                });
        }
    }

    public async Task CommitItemsAsync<T>(IEnumerable<T> items)
    {
        await ExecuteTransactionScopeAsync<T>(async (conn) =>
        {
            foreach (var item in items)
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SqliteUtil.GenerateInsertCommand<T>();
                    command.CommandType = System.Data.CommandType.Text;
                    foreach (var parameter in SqliteUtil.GenerateInsertDatabaseParameters<T>(item))
                    {
                        var param = command.CreateParameter();
                        param.ParameterName = parameter.Key;
                        param.Value = parameter.Value;
                        command.Parameters.Add(param);
                    }
                    await command.ExecuteNonQueryAsync();

                }
            }
        });
    }
}
