using System.IO.Compression;
using System.Runtime.CompilerServices;
using Core.Models;

namespace Console.Models;

public class DataService
{
    private readonly Settings _settings;

    public DataService(Settings settings)
    {
        _settings = settings;
    }

    public IAsyncEnumerable<Account> ReadAccountsAsync(
        string filename,
        string subpath = "",
        [CallerMemberName] string? name = null)
        => ReadAsync(filename, subpath, name)
            .Select(x => x.Split(':'))
            .Where(x => x.Length == 2)
            .Select(x => new Account(x[0], x[1]));
    
    public async IAsyncEnumerable<string> ReadAsync(
        string filename,
        string subpath = "",
        [CallerMemberName] string? name = null) 
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

    public async Task SaveZipAsync(
        string filename,
        string subpath,
        IEnumerable<string>[] lines,
        bool onefile = false,
        [CallerMemberName] string? name = null)
    {
        var dir = CreateDirectory(_settings.Path, "", name!);
        
        if (dir is null) return;

        int filenum = 0, index = 0;

        while (index < lines.Length)
        {
            var file = new FileStream($"{Path.Combine(dir, subpath)}{filenum}.zip", FileMode.OpenOrCreate);
            var zip = new ZipArchive(file, ZipArchiveMode.Create);

            for (; index < lines.Length; index++)
            {
                var entry = zip.CreateEntry($"{filename}{index}.txt");

                await using var writer = new StreamWriter(entry.Open());

                foreach (var str in lines.ElementAt(index))
                {
                    await writer.WriteLineAsync(str);
                }

                if (!onefile && 1024 * 1024 * 18 < file.Length) break; 
            }
            
            zip.Dispose();
            await file.DisposeAsync();
            
            filenum++;
        }
    }
    
    public async Task SaveAsync(
        string filename, 
        IEnumerable<string?> lines,
        string subpath = "",
        bool append = false,
        [CallerMemberName] string? name = null) 
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

    #region Default Files

    public Task<StreamReader?> CreateReaderAsync(
        string filename,
        string subpath = "",
        [CallerMemberName] string? name = null)
    {
        var file = CreateStream(filename, subpath, name!, false);
        
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
        var dir = CreateDirectory(_settings.Path, subpath, name);
        
        if (dir is null) return null;

        var filepath = Path.Combine(dir, $"{filename}.txt");
        
        if (clear && !File.Exists(filepath)) File.Create(filepath).Close();

        return new FileStream(filepath, clear ? FileMode.Truncate : FileMode.OpenOrCreate);
    }

    #endregion

    private static string? CreateDirectory(string logs, string subpath, string name)
    {
        var dir = new DirectoryInfo(logs);
        
        if (!dir.Exists) return null;

        name = name.Replace('_', ' ');
        var dirpath = Path.Combine(name, dir.Name, subpath);

        Directory.CreateDirectory(dirpath);

        return dirpath;
    }
}