using Core.Models.Abstractions;

namespace Console.Models;

public class LogsInfo
{
    public string Path { get; set; } = "";
    public List<ILog> Logs { get; } = new();
}