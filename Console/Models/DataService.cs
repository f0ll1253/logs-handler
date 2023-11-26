using System.Runtime.CompilerServices;

namespace Console.Models;

public class DataService
{
    private readonly Settings _settings;

    public DataService(Settings settings)
    {
        _settings = settings;
    }

    public async IAsyncEnumerable<string> ReadAsync(
        string filename,
        string subpath = "",
        [CallerMemberName] string? name = null
        )
    {
        using var reader = await CreateReaderAsync(filename, subpath, name);

        if (reader is null) yield break;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            
            if (line is null) continue;
            
            yield return line;
        }
    }
    
    public async Task SaveAsync(
        string filename, 
        IEnumerable<string?> lines,
        string subpath = "",
        bool append = false,
        [CallerMemberName] string? name = null
        ) 
    {
        await using var writer = await CreateWriterAsync(filename, subpath, append, name);
        
        if (writer is null) return;

        await writer.FlushAsync();

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            
            await writer.WriteLineAsync(line);
        }
        
        writer.Close();
    }

    public Task<StreamReader?> CreateReaderAsync(
        string filename,
        string subpath = "",
        [CallerMemberName] string? name = null)
    {
        var file = CreateStream(filename, subpath, name, false);
        
        return Task.FromResult(file is null ? null : new StreamReader(file));
    }
    
    public Task<StreamWriter?> CreateWriterAsync(
        string filename,
        string subpath = "",
        bool append = false,
        [CallerMemberName] string? name = null)
    {
        var file = CreateStream(filename, subpath, name!, !append);
        
        return Task.FromResult(file is null ? null : new StreamWriter(file));
    }

    private FileStream? CreateStream(string filename, string subpath, string name, bool clear)
    {
        var dir = new DirectoryInfo(_settings.Path);
        
        if (!dir.Exists) return null;

        name = name.Replace('_', ' ');
        var dirpath = Path.Combine(name, dir.Name, subpath);
        var filepath = Path.Combine(dirpath, $"{filename}.txt");

        Directory.CreateDirectory(dirpath);
        
        if (clear && !File.Exists(filepath)) File.Create(filepath).Close();

        return new FileStream(filepath, clear ? FileMode.Truncate : FileMode.OpenOrCreate);
    }
}