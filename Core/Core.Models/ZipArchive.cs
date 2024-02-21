using System.Text.RegularExpressions;
using SevenZip;

namespace Core.Models;

public class ZipArchive(string zippath)
{
    private readonly Regex _isMarked = new(@" (\([0-9]+\))$");
    private readonly SevenZipCompressor _compressor = new()
    {
        EventSynchronization = EventSynchronizationStrategy.AlwaysAsynchronous,
        CompressionMode = CompressionMode.Create,
        ArchiveFormat = OutArchiveFormat.Zip,
        CompressionMethod = CompressionMethod.Lzma2,
        EncryptHeaders = true,
        DirectoryStructure = true,
        PreserveDirectoryRoot = true,
        CompressionLevel = CompressionLevel.Ultra
    };

    public List<string> Entities { get; } = new();

    public async Task AddFilesAsync(Dictionary<string, Stream> dictionary, bool disposeStreams = true)
    {
        Entities.AddRange(dictionary.Select(x => x.Key));
        
        _compressor.CompressStreamDictionary(dictionary
            .Where(x => x.Value.Length > 128)
            .ToDictionary(x => x.Key, x => x.Value), 
            zippath);
        
        if (disposeStreams)
            foreach (var (filename, stream) in dictionary)
            {
                await stream.DisposeAsync();
            }
    }
    
    public async Task<bool> AddDirectoryAsync(string path)
    {
        var info = new DirectoryInfo(path);
        
        if (!info.Exists || info.GetFileSystemInfos().Length == 0) return false;

        if (Entities.Contains(info.Name))
        {
            if (_isMarked.IsMatch(info.Name)) path = path[path.LastIndexOf('\\')..path.LastIndexOf(' ')];

            path += $" ({Entities.Count})";
            info.MoveTo(path);
        }
        
        Entities.Add(info.Name);

        await _compressor.CompressDirectoryAsync(path, zippath);
        _compressor.CompressionMode = CompressionMode.Append;

        return true;
    }
}