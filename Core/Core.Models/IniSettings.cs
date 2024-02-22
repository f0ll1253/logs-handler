using System.Reflection;

namespace Core.Models;

public abstract class IniSettings : IDisposable
{
    private readonly string _path;
    private readonly Dictionary<string, PropertyInfo> _properties;

    protected IniSettings(string path)
    {
        this._properties = GetType().GetProperties().ToDictionary(x => x.Name, x => x);
        this._path = path;
    }

    public void Dispose() =>
        Save();

    ~IniSettings() =>
        Dispose();

    public void Initialize() =>
        Load(this._path);

    public void Save()
    {
        if (this._path[this._path.LastIndexOf('.')..] != ".ini")
            throw new ArgumentException("File is not .ini type configuration", nameof(this._path));

        if (!File.Exists(this._path)) File.Create(this._path).Close();

        using var writer = new StreamWriter(this._path);
        writer.Flush(); // todo remove with changing values in file (now data rewriting)

        SaveObject(writer, this);

        foreach (var prop in this._properties.Where(x => x.Value.PropertyType.IsPointer).Select(x => x.Value))
            SaveObject(writer, prop.GetValue(this)!, prop.Name);
    }

    private void Load(string path)
    {
        if (path[path.LastIndexOf('.')..] != ".ini")
            throw new ArgumentException("File is not .ini type configuration", nameof(path));

        if (!File.Exists(path)) File.Create(path).Close();

        using var reader = new StreamReader(path);
        string currentSection = "";

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();

            if (line is null || string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith('[')) currentSection = line[1..^1];

            if (GetType().Name == currentSection) UpdateSelf(reader);

            if (this._properties.TryGetValue(currentSection, out var prop)) UpdateSection(reader, prop);
        }
    }

    // save

    private void SaveObject(StreamWriter writer, object obj, string? name = null)
    {
        name ??= obj.GetType().Name;

        writer.WriteLine($"[{name}]");

        foreach (var prop in obj.GetType().GetProperties())
            if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(int) ||
                prop.PropertyType == typeof(bool))
                writer.WriteLine($"{prop.Name}={prop.GetValue(obj)}");

        writer.WriteLine();
    }

    // update
    private void UpdateSelf(StreamReader reader) =>
        UpdateObject(reader, this);

    private void UpdateSection(StreamReader reader, PropertyInfo prop) =>
        UpdateObject(reader, prop.GetValue(this)!);

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
        } while (line is { Length: 2 });
    }
}