using System.Reflection;

namespace Core.Models;

public abstract class IniSettings : IDisposable
{
    private readonly Dictionary<string, PropertyInfo> _properties;
    private readonly string _path;

    protected IniSettings(string path)
    {
        _properties = this.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
        _path = path;
    }

    ~IniSettings()
    {
        Dispose();
    }

    public void Initialize() => Load(_path);

    public void Dispose() => Save();

    public void Save()
    {
        if (_path[_path.LastIndexOf('.')..] != ".ini") throw new ArgumentException("File is not .ini type configuration", nameof(_path));
        
        if (!File.Exists(_path)) File.Create(_path).Close();
        
        using var writer = new StreamWriter(_path);
        writer.Flush(); // todo remove with changing values in file (now data rewriting)
        
        SaveObject(writer, this);

        foreach (var prop in _properties.Where(x => x.Value.PropertyType.IsPointer).Select(x => x.Value))
        {
            SaveObject(writer, prop.GetValue(this)!, prop.Name);
        }
    }
    
    private void Load(string path)
    {
        if (path[path.LastIndexOf('.')..] != ".ini") throw new ArgumentException("File is not .ini type configuration", nameof(path));

        if (!File.Exists(path)) File.Create(path).Close();

        using var reader = new StreamReader(path);
        var currentSection = "";

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            
            if (line is null || string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith('[')) currentSection = line[1..^1];

            if (GetType().Name == currentSection) UpdateSelf(reader);
            
            if (_properties.TryGetValue(currentSection, out var prop)) UpdateSection(reader, prop);
        }
    }
    
    // save

    private void SaveObject(StreamWriter writer, object obj, string? name = null)
    {
        name ??= obj.GetType().Name;
        
        writer.WriteLine($"[{name}]");
        
        foreach (var prop in obj.GetType().GetProperties())
        {
            if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(int) || prop.PropertyType == typeof(bool)) writer.WriteLine($"{prop.Name}={prop.GetValue(obj)}");
        }
        
        writer.WriteLine();
    }
    
    // update
    private void UpdateSelf(StreamReader reader) => UpdateObject(reader, this);

    private void UpdateSection(StreamReader reader, PropertyInfo prop) => UpdateObject(reader, prop.GetValue(this)!);

    private void UpdateObject(StreamReader reader, object obj)
    {
        var properties = obj.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
        string[]? line = null;

        do
        {
            line = reader.ReadLine()?.Split('=');
            
            if (line is null || line.Length != 2 || !properties.TryGetValue(line[0], out var prop)) continue;

            if (prop.PropertyType == typeof(string)) prop.SetValue(this, line[1]);
            else if (prop.PropertyType == typeof(int)) prop.SetValue(this, int.Parse(line[1]));
            else if (prop.PropertyType == typeof(bool)) prop.SetValue(this, bool.Parse(line[1]));

        } while (line is {Length:2});
    }
}