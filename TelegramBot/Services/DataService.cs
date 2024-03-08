using CG.Web.MegaApiClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers.Zip;
using TelegramBot.Data;
using TL;
using WTelegram;
using File = System.IO.File;

namespace TelegramBot.Services;

public class DataService(Client client, IMegaApiClient mega, IConfiguration config, AppDbContext context)
{
    public string BaseFolder { get; } = config["BaseFolder"]!;

    public string GetExtractedPath(string logsname) =>
        Path.Combine(BaseFolder, "Extracted", logsname);

    public string GetServicePath(string logsname, string name) =>
        Path.Combine(BaseFolder, name, logsname);

    public IEnumerable<string> AvailableLogs(int start = 0, int count = -1) =>
        Directory.GetDirectories(Path.Combine(BaseFolder, "Extracted"))
                 .Select(x => new DirectoryInfo(x))
                 .OrderByDescending(x => x.CreationTimeUtc)
                 .Take(count == -1 ? Range.All : new Range(start * count, start * count + count))
                 .Select(x => x.Name);

    #region data manipulation

    public async Task<string> SaveAsync(
        string filename,
        IEnumerable<string> lines,
        string name = "")
    {
        var file = new FileInfo(Path.Combine(BaseFolder, name, $"{filename}.txt"));

        file.Directory?.Create();

        using var filestream = file.Create();
        using var writer = new StreamWriter(filestream);

        foreach (string line in lines) await writer.WriteLineAsync(line);

        return file.FullName;
    }
    
    public async Task<string> SaveZipAsync(
        string logsname,
        string filename,
        string subpath,
        IEnumerable<string>[] data,
        string name)
    {
        // init dir
        string dir = Path.Combine(BaseFolder, name, logsname),
            path = Path.Combine(dir, $"{subpath}.zip");;

        Directory.CreateDirectory(dir);

        using var zip = ArchiveFactory.Create(ArchiveType.Zip);
        
        for (int i = 0; i < data.Length; i++)
        {
            var stream = new MemoryStream();
            
            using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                await writer.WriteAsync(string.Join('\n', data[i]));
            }
            
            stream.Position = 0;
            
            zip.AddEntry($"{filename}{i}.txt", stream, true);
        }
        
        zip.SaveTo(path, new ZipWriterOptions(CompressionType.LZMA)
        {
            LeaveStreamOpen = true
        });

        return path;
    }

    public string? ExtractFiles(string filepath, string? password)
    {
        var fileinfo = new FileInfo(filepath);
        string filename = fileinfo.Name[..fileinfo.Name.LastIndexOf('.')];
        string destinationPath = Path.Combine(BaseFolder, "Extracted", filename);

        if (Directory.Exists(destinationPath)) return destinationPath;

        Directory.CreateDirectory(destinationPath);

        using var archive = ArchiveFactory.Open(filepath, new ReaderOptions
        {
            Password = password,
            LookForHeader = true,
            DisableCheckIncomplete = true
        });
        
        var destinationInfo = new DirectoryInfo(destinationPath);

        try
        {
            archive.ExtractToDirectory(archive.Entries.Any(x => x.IsDirectory && x.Key == destinationInfo.Name)
                ? destinationInfo.Parent!.FullName
                : destinationInfo.FullName);
        }
        catch
        {
            Directory.Delete(destinationPath, true);
            throw;
        }
        
        return destinationPath;
    }

    #endregion

    #region network

    public async Task<string> GetShareLinkAsync(string filepath)
    {
        var node = await mega.UploadFileAsync(filepath, (await mega.GetNodesAsync()).First());

        return (await mega.GetDownloadLinkAsync(node)).ToString();
    }
    
    public async Task<bool> TrySendUploadedAsync(InputPeer peer, string logsname, string category)
    {
        var uploaded = await context.Files!
            .Where(x => x.LogsName == logsname)
            .Where(x => x.Category == category)
            .ToListAsync();

        if (!uploaded.Any())
            return false;

        foreach (var group in uploaded
                     .Select((x, y) => new { Index = y, Value = x })
                     .GroupBy(x => x.Index / 8)
                     .Select(x => x.Select(x => x.Value)))
            await client.SendAlbumAsync(
                peer,
                group
                    .Select(x => (InputMedia) new InputDocument
                    {
                        id = x.Id,
                        access_hash = x.AccessHash,
                        file_reference = x.FileReference
                    })
                    .ToList(),
                caption: $"#Cookies\n{logsname}",
                entities: 
                [
                    new MessageEntityHashtag
                    {
                        length = "#Cookies".Length,
                        offset = 0
                    }
                ]
            );

        return true;
    }

    public async Task SendFilesAsync(InputPeer peer, IEnumerable<string> files, string logsname, string category)
    {
        if (!files.Any())
        {
            await client.SendMessageAsync(peer, "Sequence contains no elements");
            
            return;
        }

        var messages = new List<Message>();

        foreach (var group in files
                     .Select((x, y) => new { Index = y, Value = x })
                     .GroupBy(x => x.Index / 8)
                     .Select(x => x.Select(x => x.Value)))
            messages.AddRange(await client.SendAlbumAsync(peer,
                await group
                    .ToAsyncEnumerable()
                    .SelectAwait(async x => await client.UploadFileAsync(x))
                    .Select(x => new InputMediaUploadedDocument(x, "application/zip"))
                    .ToArrayAsync(),
                caption: $"#{category}\n{logsname}",
                entities:
                [
                    new MessageEntityHashtag
                    {
                        length = $"#{category}".Length,
                        offset = 0
                    }
                ]
            ));

        await SaveFilesData(messages, logsname, category);
    }

    public async Task SendFileAsync(InputPeer peer, string filepath, string message = "")
    {
        var uploaded = await client.UploadFileAsync(filepath);

        await client.Messages_SendMedia(peer,
            new InputMediaUploadedDocument(
                uploaded,
                ""),
            message,
            Random.Shared.NextInt64(),
            clear_draft: true);
    }

    public async Task<string?> DownloadDocumentAsync(Document document)
    {
        if (document.mime_type is not ("application/zip" or "application/vnd.rar"))
            return null;

        string filepath = Path.Combine(BaseFolder, "Logs", document.Filename);

        if (File.Exists(filepath))
            return filepath;

        await using var file = new FileStream(filepath, FileMode.Create);
        await client.DownloadFileAsync(document, file);

        return filepath;
    }

    #endregion
    
    private async Task SaveFilesData(IEnumerable<Message> messages, string logsname, string category)
    {
        var medias = messages
                     .Select(x => (MessageMediaDocument)x.media)
                     .Select(x => (Document)x.document);

        await context.AddRangeAsync(medias.Select(x => new TelegramBot.Data.File
        {
            Id = x.id,
            AccessHash = x.access_hash,
            FileReference = x.file_reference,
            Type = x.Filename,
            Category = category,
            LogsName = logsname
        }));

        await context.SaveChangesAsync();
    }
}