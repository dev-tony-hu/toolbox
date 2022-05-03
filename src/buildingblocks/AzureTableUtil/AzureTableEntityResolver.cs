internal static class AzureTableEntityResolver
{
    private static string ReservedKey_TimeStamp = "Timestamp";
    private static string ReservedKey_PartitionKey = "PartitionKey";
    private static string ReservedKey_RowKey = "RowKey";
    private static Dictionary<string, Dictionary<string, PropertyInfo>> cache = new Dictionary<string, Dictionary<string, PropertyInfo>>();
    public static T Convert<T>(this TableEntity entity) where T : ITableEntity
    {
        if (entity == null)
        {
            return default;
        }
        Dictionary<string, PropertyInfo> properties;
        lock (cache)
        {
            var typeName = typeof(T).FullName;
            if (!cache.TryGetValue(typeName, out properties))
            {
                properties = typeof(T).GetProperties().Where(t => t.SetMethod != null).ToDictionary(t => t.Name);
                cache.TryAdd(typeName, properties);
            }
        }

        var obj = Activator.CreateInstance<T>();
        foreach (var property in properties)
        {
            if (entity.ContainsKey(property.Key))
            {
                if (property.Value.PropertyType == typeof(DateTime))
                {
                    property.Value.SetValue(obj, entity.GetDateTime(property.Key));
                }
                else
                {
                    property.Value.SetValue(obj, entity[property.Key]);
                }
            }
        }
        return obj;
    }
}
