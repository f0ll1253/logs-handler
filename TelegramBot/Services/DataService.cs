using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;
using SevenZip;
using TL;
using WTelegram;

namespace TelegramBot.Services;

public class DataService(Client client, Random random, IConfiguration config)
{
    private readonly string _baseFolder = config["BaseFolder"]!;

    public async Task<string> SaveZipAsync(
        string zipname,
        string filename,
        string subpath,
        IEnumerable<string>[] data,
        [CallerMemberName] string name = "")
    {
        // init dir
        var dir = Path.Combine(_baseFolder, name, zipname);

        Directory.CreateDirectory(dir);

        var zip = new SevenZipCompressor();
        var map = new Dictionary<string, Stream>();

        for (var i = 0; i < data.Length; i++)
        {
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory);

            foreach (var str in data[i])
            {
                await writer.WriteLineAsync(str);
            }
            
            writer.Flush();
            memory.Position = 0;
            
            map.Add($"{filename}{i}.txt", memory);
        }

        var zippath = Path.Combine(dir, $"{subpath}.zip");
        zip.CompressStreamDictionary(map, zippath);

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

        var zip = new SevenZipExtractor(filepath, password ?? "");
        
        try
        {
            if (!zip.Check()) throw new Exception($"Invalid password: {password}");

            var foldername = extractFolder[(extractFolder.LastIndexOf('\\') + 1)..];
            var copyFolder = (ArchiveFileInfo?) zip.ArchiveFileData.FirstOrDefault(x => x.IsDirectory && x.FileName == foldername);
            
            if (copyFolder.HasValue)
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
}