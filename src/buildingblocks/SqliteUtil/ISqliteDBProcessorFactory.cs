public interface ISqliteDBProcessorFactory
{
    ISqliteDBProcessor CreateInstance(string path);
}