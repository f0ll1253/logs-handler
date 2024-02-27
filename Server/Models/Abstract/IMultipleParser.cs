namespace Server.Models.Abstract;

public interface IMultipleParser<T>
    where T : class
{
    IAsyncEnumerable<T> FromLogs(string logs);

    IAsyncEnumerable<T> FromLog(string log);
}