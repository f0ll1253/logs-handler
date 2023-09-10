using Core.Models.Logs.Abstractions;

namespace Console.Models;

public class LogsInfo
{
    public string Path { get; set; } = "";
    public IAsyncEnumerable<ILog> Logs { get; set; } = AsyncEnumerable.Empty<ILog>();
}