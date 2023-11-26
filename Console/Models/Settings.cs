using Core.Models;

namespace Console.Models;

public class Settings : IniSettings
{
    public Settings() : base("settings.ini")
    {
    }
    
    public string Path { get; set; } = "";
    public string ProxyPath { get; set; } = "";
}