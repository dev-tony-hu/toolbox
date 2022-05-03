public class TableServiceUtil
{
    protected TableServiceClient TableServiceClient { get; set; }
    /// <summary>
    /// use DefaultAzureCredential
    /// </summary>
    /// <param name="endpoint"></param>
    public TableServiceUtil(Uri endpoint)
    {
        TableServiceClient = new TableServiceClient(endpoint, new DefaultAzureCredential());
    }

    public TableServiceUtil(Uri endpoint, TokenCredential tokenCredential)
    {
        TableServiceClient = new TableServiceClient(endpoint, tokenCredential);
    }

    public async ValueTask<bool> TableExist(string name, CancellationToken cancellationToken = default)
    {
        await foreach (var item in TableServiceClient.QueryAsync($"Name -eq {name}", 1, cancellationToken))
        {
            return item!=null;
        }
        return false;
    }

    public async IAsyncEnumerable<string> Query(string filter = default, int maxPerPage = 1000, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var tableItem in TableServiceClient.QueryAsync(filter, maxPerPage, cancellationToken))
        {
            yield return tableItem.Name;
        }
    }
}