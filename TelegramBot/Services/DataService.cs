using System.IO.Compression;
using System.Runtime.CompilerServices;
using CG.Web.MegaApiClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using TL;
using WTelegram;

namespace TelegramBot.Services;

public class DataService(Client client, IMegaApiClient mega, IConfiguration config)
{
    public string BaseFolder { get; } = config["BaseFolder"]!;

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
               archive = Path.Combine(dir, $"{subpath}.zip");;

        Directory.CreateDirectory(dir);

        using var zip = ZipFile.Open(archive, ZipArchiveMode.Create);

        for (int i = 0; i < data.Length; i++)
        {
            var entry = zip.CreateEntry($"{filename}{i}.txt");

            using (var stream = entry.Open())
                using (var writer = new StreamWriter(stream))
                    foreach (string str in data[i])
                        await writer.WriteLineAsync(str);
        }

        return archive;
    }

    public string? ExtractFiles(string filepath, string? password)
    {
        var fileinfo = new FileInfo(filepath);
        string filename = fileinfo.Name[..fileinfo.Name.LastIndexOf('.')];
        string destinationPath = Path.Combine(BaseFolder, "Extracted", filename);

        if (Directory.Exists(destinationPath)) return destinationPath;

        Directory.CreateDirectory(destinationPath);

        using var zip = Ionic.Zip.ZipFile.Read(filepath);

        zip.Password = password ?? "";
        
        try
        {
            var destinationInfo = new DirectoryInfo(destinationPath);
            
            if (zip.Entries.Any(x => x.IsDirectory && x.FileName == destinationInfo.Name))
                zip.ExtractAll(destinationInfo.Parent!.FullName);
            else
                zip.ExtractAll(destinationPath);
        }
        catch (Exception e)
        {
            Directory.Delete(destinationPath);
            Log.Error(e.ToString());
            return null;
        }
        finally
        {
            zip.Dispose();
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
        if (document.mime_type is not ("zip" or "rar" or "7z"))
            return null;

        string filepath = Path.Combine(BaseFolder, "Logs", document.Filename);

        if (File.Exists(filepath))
            return filepath;

        await using var file = new FileStream(filepath, FileMode.Create);
        await client.DownloadFileAsync(document, file);

        return filepath;
    }

    #endregion
}