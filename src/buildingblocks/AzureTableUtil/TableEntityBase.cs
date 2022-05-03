public abstract class TableEntityBase : ITableEntity
{
    public virtual string PartitionKey { get; set; } = "";

    public virtual string RowKey { get; set; } = "";
    public virtual DateTimeOffset? Timestamp { get; set; }
    public virtual ETag ETag { get; set; }
}
