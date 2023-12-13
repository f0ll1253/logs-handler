using System.Runtime.CompilerServices;
using CG.Web.MegaApiClient;
using Core.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using SevenZip;
using TL;
using WTelegram;

namespace TelegramBot.Services;

public class DataService(Client client, IMegaApiClient mega, Random random, IConfiguration config)
{
    private readonly string _baseFolder = config["BaseFolder"]!;
    
    public string CreateZipPath(
        string logsname,
        [CallerMemberName] string name = "")
    {
        var dir = Path.Combine(_baseFolder, name);

        Directory.CreateDirectory(dir);

        return Path.Combine(dir, $"{logsname}.zip");
    }
    
    public string? GetLogsPath(string logsname) => Directory.GetFiles(Path.Combine(_baseFolder, "Logs")).FirstOrDefault(x => x[(x.LastIndexOf('\\')+1)..x.LastIndexOf('.')] == logsname);

    public string GetExtractedPath(string logsname) => Path.Combine(_baseFolder, "Extracted", logsname);
    
    public async Task<string> SaveAsync(
        string filename,
        IEnumerable<string> lines,
        [CallerMemberName] string name = "")
    {
        var file = new FileInfo(Path.Combine(_baseFolder, name, $"{filename}.txt"));
        
        file.Directory?.Create();

        using var filestream = file.Create();
        using var writer = new StreamWriter(filestream);

        foreach (var line in lines)
        {
            await writer.WriteLineAsync(line);
        }

        return file.FullName;
    }
    
    public IEnumerable<string> AvailableLogs(int start = 0, int count = -1) 
        => Directory.GetDirectories(Path.Combine(_baseFolder, "Extracted"))
            .Select(x => new DirectoryInfo(x))
            .OrderByDescending(x => x.CreationTimeUtc)
            .Take(count == -1 ? Range.All : new Range(start * count, start * count + count))
            .Select(x => x.Name);
    
    public async Task<string> SaveZipAsync(
        string logsname,
        string filename,
        string subpath,
        IEnumerable<string>[] data,
        [CallerMemberName] string name = "")
    {
        // init dir
        var dir = Path.Combine(_baseFolder, name, logsname);

        Directory.CreateDirectory(dir);
        
        var map = new Dictionary<string, Stream>();

        for (var i = 0; i < data.Length; i++)
        {
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory);

            foreach (var str in data[i])
            {
                await writer.WriteLineAsync(str);
            }
            
            map.Add($"{filename}{i}.txt", memory);
        }
        
        var zippath = Path.Combine(dir, $"{subpath}.zip");
        await new ZipArchive(zippath).AddFilesAsync(map);

        return zippath;
    }
    
    public async Task<string?> ExtractFilesAsync(InputPeer peer, string filepath, string? password)
    {
        var fileinfo = new FileInfo(filepath);
        var filename = fileinfo.Name[..fileinfo.Name.LastIndexOf('.')];
        var extractFolder = Path.Combine(_baseFolder, "Extracted", filename);
        
        if (Directory.Exists(extractFolder)) return extractFolder;

        await client.Messages_SendMessage(peer, $"Extracting files from {filename}", random.NextInt64());

        Directory.CreateDirectory(extractFolder);

        var zip = new SevenZipExtractor(filepath, password ?? "")
        {
            EventSynchronization = EventSynchronizationStrategy.AlwaysAsynchronous,
        };
        
        try
        {
            if (!zip.Check()) throw new Exception($"Invalid password: {password}");

            if (zip.ArchiveFileData.First().FileName.Contains(extractFolder[(extractFolder.LastIndexOf('\\') + 1)..]))
            {
                await zip.ExtractArchiveAsync(extractFolder[..extractFolder.LastIndexOf('\\')]);
            }
            else
            {
                await zip.ExtractArchiveAsync(extractFolder);
            }
        }
        catch (Exception e)
        {
            Directory.Delete(extractFolder);
            Log.Error(e.ToString());
            await client.Messages_SendMessage(peer, $"Error while extracting files from {filename}", random.NextInt64());
            return null;
        }
        finally
        {
            zip.Dispose();
        }
        
        return extractFolder;
    }
    
    #region network
    
    public async Task<string> GetShareLinkAsync(string filepath)
    {
        var node = await mega.UploadFileAsync(filepath, (await mega.GetNodesAsync()).First());

        return (await mega.GetDownloadLinkAsync(node)).ToString();
    }

    public async Task SendFileAsync(InputPeer peer, string filepath, string message = "")
    {
        var uploaded = await client.UploadFileAsync(filepath);

        await client.Messages_SendMedia(peer,
            new InputMediaUploadedDocument(
                uploaded,
                ""),
            message,
            random.NextInt64(),
            clear_draft: true);
    }
    
    public async Task<string?> DownloadFileFromMessageAsync(InputPeer peer, UpdateNewMessage update)
    {
        var message = (Message) update.message;

        if (!message.flags.HasFlag(Message.Flags.has_media))
        {
            await client.Messages_SendMessage(peer, "Error zip/rar archive not found", random.NextInt64());
            return null;
        }

        var media = (MessageMediaDocument) message.media;
        var document = (Document) media.document;

        if (document.Filename.Split('.').LastOrDefault() is not ("zip" or "rar" or "7z"))
        {
            await client.Messages_SendMessage(peer, "Error zip/rar archive not found", random.NextInt64());
            return null;
        }

        var filepath = Path.Combine(_baseFolder, "Logs", document.Filename);

        if (File.Exists(filepath)) return filepath;
        
        await client.Messages_SendMessage(peer, "Downloading file", random.NextInt64());
        
        await using var file = new FileStream(filepath, FileMode.Create);
        await client.DownloadFileAsync(document, file);

        return filepath;
    }

    #endregion
}