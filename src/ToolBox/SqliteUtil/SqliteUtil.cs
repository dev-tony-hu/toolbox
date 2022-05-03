internal static class SqliteUtil
{
    private static ConcurrentDictionary<Type, string> CreateTableCommandCache = new ConcurrentDictionary<Type, string>();
    private static ConcurrentDictionary<Type, string> InsertTableCommandCache = new ConcurrentDictionary<Type, string>();
    private static ConcurrentDictionary<Type, string> ReadTableCommandCache = new ConcurrentDictionary<Type, string>();
    private static ConcurrentDictionary<Type, List<PropertyInfo>> AvaliablePropertyCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
    private static Dictionary<Type, string> DataMapper = new Dictionary<Type, string>()
        {
            { typeof(int), "BIGINT" },
            { typeof(uint), "BIGINT" },
            { typeof(string), "NVARCHAR(200)" },
            { typeof(bool), "BIT" },
            { typeof(DateTime), "DATETIME" },
            { typeof(float), "FLOAT" },
            { typeof(long), "BIGINT" },
            { typeof(double), "DECIMAL(18,0)" },
            { typeof(decimal), "DECIMAL(18,0)" },
            { typeof(Guid), "UNIQUEIDENTIFIER" }
        };

    private static List<PropertyInfo> GetAvaliableProperties(Type type)
    {
        if (AvaliablePropertyCache.TryGetValue(type, out List<PropertyInfo> properties))
        {
            return properties;
        }
        return AvaliablePropertyCache.GetOrAdd(type, type.GetProperties().Where(t => DataMapper.ContainsKey(t.PropertyType)).ToList());
    }

    public static string GenerateCreateTableCommand<T>()
    {
        Type type = typeof(T);
        if (CreateTableCommandCache.TryGetValue(type, out string command))
        {
            return command;
        }
        var properties = GetAvaliableProperties(type);
        StringBuilder builder = new StringBuilder();
        builder.Append($"CREATE TABLE IF Not Exists {typeof(T).Name}(");
        List<string> columns = new List<string>();
        foreach (var property in properties)
        {
            columns.Add($"{property.Name} {DataMapper[property.PropertyType]} NULL");
        }
        builder.Append(string.Join(",", columns));
        builder.AppendLine(");");
        command = builder.ToString();
        return CreateTableCommandCache.GetOrAdd(type, command);
    }

    public static T ReadObject<T>(DbDataReader reader)
    {
        dynamic obj = Activator.CreateInstance(typeof(T));
        foreach (var prop in GetAvaliableProperties(typeof(T)))
        {
            int index = reader.GetOrdinal(prop.Name);
            if (reader.IsDBNull(index))
            {
                continue;
            }
            prop.SetValue(obj, reader[prop.Name]);
        }
        return obj;
    }

    public static string GenerateReadCommand<T>()
    {
        var type = typeof(T);
        if (ReadTableCommandCache.TryGetValue(type, out string command))
        {
            return command;
        }
        return ReadTableCommandCache.GetOrAdd(type, $"Select {string.Join(",", GetAvaliableProperties(type).Select(t => t.Name))} FROM {type.Name}");
    }

    public static string GenerateInsertCommand<T>()
    {
        var type = typeof(T);
        if (InsertTableCommandCache.TryGetValue(type, out string command))
        {
            return command;
        }
        string parameters = string.Join(",", GetAvaliableProperties(type).Select(t => $"@{t.Name}"));
        return InsertTableCommandCache.GetOrAdd(type, $@"Insert into {typeof(T).Name} values({parameters})");
    }

    public static Dictionary<string, object> GenerateInsertDatabaseParameters<T>(T obj)
    {
        return GetAvaliableProperties(typeof(T)).ToList().ToDictionary(p => p.Name, p => p.GetValue(obj));
    }

}

