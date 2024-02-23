using System.Runtime.CompilerServices;
using Aspose.Zip.SevenZip;
using CG.Web.MegaApiClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using TL;
using WTelegram;

namespace TelegramBot.Services;

public class DataService(Client client, IMegaApiClient mega, IConfiguration config)
{
    private readonly string _baseFolder = config["BaseFolder"]!;

    public string CreateZipPath(
        string logsname,
        [CallerMemberName] string name = "")
    {
        string dir = Path.Combine(this._baseFolder, name);

        Directory.CreateDirectory(dir);

        return Path.Combine(dir, $"{logsname}.zip");
    }

    public string? GetLogsPath(string logsname) =>
        Directory.GetFiles(Path.Combine(this._baseFolder, "Logs"))
                 .FirstOrDefault(x => new DirectoryInfo(x).Name == logsname);

    public string GetExtractedPath(string logsname) =>
        Path.Combine(this._baseFolder, "Extracted", logsname);

    public async Task<string> SaveAsync(
        string filename,
        IEnumerable<string> lines,
        [CallerMemberName] string name = "")
    {
        var file = new FileInfo(Path.Combine(this._baseFolder, name, $"{filename}.txt"));

        file.Directory?.Create();

        using var filestream = file.Create();
        using var writer = new StreamWriter(filestream);

        foreach (string line in lines) await writer.WriteLineAsync(line);

        return file.FullName;
    }

    public IEnumerable<string> AvailableLogs(int start = 0, int count = -1) =>
        Directory.GetDirectories(Path.Combine(this._baseFolder, "Extracted"))
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
        string dir = Path.Combine(this._baseFolder, name, logsname),
               archive = Path.Combine(dir, $"{subpath}.zip");;

        Directory.CreateDirectory(dir);

        using var zip = new SevenZipArchive();

        for (int i = 0; i < data.Length; i++)
        {
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory);

            foreach (string str in data[i]) await writer.WriteLineAsync(str);

            zip.CreateEntry($"{filename}{i}.txt", memory);
        }

        zip.Save(archive);

        return archive;
    }

    public string? ExtractFiles(string filepath, string? password)
    {
        var fileinfo = new FileInfo(filepath);
        string filename = fileinfo.Name[..fileinfo.Name.LastIndexOf('.')];
        string destinationPath = Path.Combine(this._baseFolder, "Extracted", filename);

        if (Directory.Exists(destinationPath)) return destinationPath;

        Directory.CreateDirectory(destinationPath);

        var zip = new SevenZipArchive(filepath, password);
        
        try
        {
            var destinationInfo = new DirectoryInfo(destinationPath);
            
            if (zip.Entries.Any(x => x.IsDirectory && x.Name == destinationInfo.Name))
                zip.ExtractToDirectory(destinationInfo.Parent!.FullName);
            else
                zip.ExtractToDirectory(destinationPath);
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

        string filepath = Path.Combine(this._baseFolder, "Logs", document.Filename);

        if (File.Exists(filepath))
            return filepath;

        await using var file = new FileStream(filepath, FileMode.Create);
        await client.DownloadFileAsync(document, file);

        return filepath;
    }

    #endregion
}