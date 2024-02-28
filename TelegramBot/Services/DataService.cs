using System.Runtime.CompilerServices;
using CG.Web.MegaApiClient;
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

    public async Task SendFilesAsync(InputPeer peer, IEnumerable<string> files, string hashTag, string logsname)
    {
        if (files.Count() == 0)
            return;
        
        var messages = await client.SendAlbumAsync(peer,
            await files
                  .ToAsyncEnumerable()
                  .SelectAwait(async x => await client.UploadFileAsync(x))
                  .Select(x => new InputMediaUploadedDocument(x, "application/zip"))
                  .ToArrayAsync(),
            caption: $"#{hashTag}\n{logsname}",
            entities:
            [
                new MessageEntityHashtag
                {
                    length = $"#{hashTag}".Length,
                    offset = 0
                }
            ]
        );

        await SaveFilesData(messages, logsname);
    }

    public string? GetLogsPath(string logsname) =>
        Directory.GetFiles(Path.Combine(BaseFolder, "Logs"))
                 .FirstOrDefault(x => new DirectoryInfo(x).Name == logsname);

    public string GetExtractedPath(string logsname) =>
        Path.Combine(BaseFolder, "Extracted", logsname);

    public async Task<string> SaveAsync(
        string filename,
        IEnumerable<string> lines,
        [CallerMemberName] string name = "")
    {
        var file = new FileInfo(Path.Combine(BaseFolder, name, $"{filename}.txt"));

        file.Directory?.Create();

        using var filestream = file.Create();
        using var writer = new StreamWriter(filestream);

        foreach (string line in lines) await writer.WriteLineAsync(line);

        return file.FullName;
    }

    public IEnumerable<string> AvailableLogs(int start = 0, int count = -1) =>
        Directory.GetDirectories(Path.Combine(BaseFolder, "Extracted"))
                 .Select(x => new DirectoryInfo(x))
                 .OrderByDescending(x => x.CreationTimeUtc)
                 .Take(count == -1 ? Range.All : new Range(start * count, start * count + count))
                 .Select(x => x.Name);

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
    
    private async Task SaveFilesData(Message[] messages, string logsname)
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
            Category = "Cookies",
            LogsName = logsname
        }));

        await context.SaveChangesAsync();
    }
}