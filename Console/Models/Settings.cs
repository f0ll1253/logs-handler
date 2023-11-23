using Core.Models;

namespace Console.Models;

public class Settings
{
    public string Path { get; set; } = "";
    public ProxyPool Proxy { get; set; } = new ();
}