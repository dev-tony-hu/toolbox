internal class SqliteDBProcessorFactory : ISqliteDBProcessorFactory
{
    /// <summary>
    /// this path can be a folder or file path,if use file path, data will be stored in the specific file
    /// if it is a folder, will generate a file with the commit object type name in the specific folder.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public ISqliteDBProcessor CreateInstance(string path)
    {
        return new SqliteDBProcessor(path, true);
    }
}
