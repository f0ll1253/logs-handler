using System.Runtime.CompilerServices;

namespace Console.Models;

public class SaverService
{
    private readonly Settings _settings;

    public SaverService(Settings settings)
    {
        _settings = settings;
    }

    public async Task SaveAsync(
        string filename, 
        IEnumerable<string?> lines,
        string subpath = "",
        bool append = false,
        [CallerMemberName] string? name = null
        ) {
        await using var writer = await CreateStreamAsync(filename, subpath, append, name);
        
        if (writer is null) return;

        await writer.FlushAsync();

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            
            await writer.WriteLineAsync(line);
        }
        
        writer.Close();
    }

    public Task<StreamWriter?> CreateStreamAsync(
        string filename,
        string subpath = "",
        bool append = false,
        [CallerMemberName] string? name = null
        ) {
        var dir = new DirectoryInfo(_settings.Path);
        
        if (name is null || !dir.Exists) return Task.FromResult<StreamWriter?>(null);

        name = name.Replace('_', ' ');

        Directory.CreateDirectory(Path.Combine(name, dir.Name, subpath));

        return Task.FromResult<StreamWriter?>(new StreamWriter(Path.Combine(name, dir.Name, subpath, $"{filename}.txt"), append));
    }
}