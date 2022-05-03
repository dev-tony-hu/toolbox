public static class SqliteDBProcessorFactoryExtension
{
    public static IServiceCollection AddSqliteDBProceesorFactory(this IServiceCollection services)
    {
        return services.AddSingleton<ISqliteDBProcessorFactory, SqliteDBProcessorFactory>();
    }
}
