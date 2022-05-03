public interface ISqliteDBProcessor
{
    void CommitItems<T>(List<T> items);
    IEnumerable<T> ReadItems<T>();
#if NET5_0_OR_GREATER
    Task CommitItemsAsync<T>(IEnumerable<T> items);

    IAsyncEnumerable<T> ReadItemsAsync<T>();
#endif
}