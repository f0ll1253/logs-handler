using Core.Models;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;

namespace Core.Parsers;

public static class CookiesParser
{
    public static void CookiesFromFile(this IEnumerable<CookieDomain> domains, string path, Dictionary<CookieDomain, StreamWriter> writers)
    {
        using var reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();

            if (line is not { Length: > 1 }) continue;

            int index = line.IndexOf('\t');
            string domain = line[..(index == -1 ? 1 : index)];

            if (
                domains.FirstOrDefault(
                    x => x.IsFull
                        ? x.Domains.FirstOrDefault(a => domain == $".{a}") is { }
                        : x.Domains.FirstOrDefault(a => domain.Contains(a)) is { }) is not
                { } pickedCookie) continue;

            writers[pickedCookie].WriteLine(line);
        }
    }
    
    public static IEnumerable<KeyValuePair<CookieDomain, Stream>> CookiesFromLog(this IEnumerable<CookieDomain> domains, string path, Dictionary<CookieDomain, IWritableArchive> archives)
    {
        var writers = archives.ToDictionary(x => x.Key, x => new StreamWriter(new MemoryStream()));
        var cookies = Path.Combine(path, "Cookies");
        
        if (!Directory.Exists(cookies)) yield break;
        
        // parse from files
        foreach (var filepath in Directory.GetFiles(cookies))
            domains.CookiesFromFile(filepath, writers);
        
        // create entries
        foreach (var (domain, writer) in writers)
        {
            if (writer.BaseStream.Length == 0)
            {
                writer.BaseStream.Dispose();
                
                continue;
            }

            writer.BaseStream.Position = 0;

            yield return new KeyValuePair<CookieDomain, Stream>(domain, writer.BaseStream);
        }
    }
    
    public static async Task<IEnumerable<string>> CookiesFromLogs(this IEnumerable<CookieDomain> domains, string path, string output)
    {
        var archives = domains.ToDictionary(x => x, x => ArchiveFactory.Create(ArchiveType.Zip));
        var streams = new List<KeyValuePair<CookieDomain, Stream>>();
        
        var logs = Directory.GetDirectories(path);
        var @event = new ManualResetEvent(false);
        var count = logs.Length; 
        
        Directory.CreateDirectory(output);
        
        // parse from logs
        await foreach (var logpath in logs.ToAsyncEnumerable())
            Task.Run(() =>
            {
                streams.AddRange(domains.CookiesFromLog(logpath, archives));

                if (Interlocked.Decrement(ref count) == 0) @event.Set();
            });

        @event.WaitOne();
        @event.Reset();
        
        // write entries
        count = streams.Count;
        
        foreach (var (domain, archive) in archives)
            Task.Run(() =>
            {
                foreach (var stream in streams
                             .Where(x => x.Key == domain)
                             .Select(x => x.Value))
                {
                    archive.AddEntry($"{Guid.NewGuid().ToString()}.txt", stream, true);
                    
                    if (Interlocked.Decrement(ref count) == 0) @event.Set();
                }
            });

        @event.WaitOne();
        @event.Reset();
        
        // save archives
        var paths = new List<string>();
        count = archives.Count;

        foreach (var (domain, archive) in archives)
            Task.Run(() =>
            {
                paths.Add(archive.SaveTo(output, domain.Domains[0]));

                if (Interlocked.Decrement(ref count) == 0) @event.Set();
            });

        @event.WaitOne();
        @event.Dispose();
        
        return paths;
    }

    private static string SaveTo(this IWritableArchive archive, string output, string domain)
    {
        var path = Path.Combine(output, $"{domain}_{Guid.NewGuid().ToString()}.zip");
        archive.SaveTo(path, new ZipWriterOptions(CompressionType.LZMA));
        return path;
    }
}