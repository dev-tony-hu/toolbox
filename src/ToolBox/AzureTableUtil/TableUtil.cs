public class TableUtil
{
    protected TableClient TableClient { get; set; }
    public TableUtil(Uri endpoint, string tableName)
    {
        TableClient = new TableClient(endpoint, tableName, new DefaultAzureCredential());
    }

    public TableUtil(Uri endpoint, string tableName, TokenCredential credential)
    {
        TableClient = new TableClient(endpoint, tableName, credential);
    }

    public async Task CreateIfNotExist(CancellationToken cancellationToken = default)
    {
        await TableClient.CreateIfNotExistsAsync(cancellationToken);
    }

    public async IAsyncEnumerable<T> Query<T>(string? filter = default, int batchCount = 1000, IEnumerable<string> selectedFields = default, [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : ITableEntity, new()
    {
        await foreach (var item in TableClient.QueryAsync<TableEntity>(filter, batchCount > 1000 ? 1000 : batchCount, selectedFields, cancellationToken))
        {
            yield return item.Convert<T>();
        }
    }
    public async ValueTask UpsertEntityAsync<T>(T item, CancellationToken cancellationToken = default) where T : ITableEntity, new()
    {
        await TableClient.UpsertEntityAsync<T>(item,TableUpdateMode.Replace, cancellationToken);
    }

    public async ValueTask SubmitTransactionAsync(IEnumerable<TableTransactionAction> items, CancellationToken cancellationToken = default)
    {
        await TableClient.SubmitTransactionAsync(items,cancellationToken);
    }

    public async ValueTask AddAsync<T>(T item, CancellationToken cancellationToken = default) where T : ITableEntity, new()
    {
           await TableClient.AddEntityAsync<T>(item,cancellationToken);
    }
}