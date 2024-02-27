using System.IO.Compression;

namespace Server.Models.FileManagers;

public abstract class FileManager<T>(string? volume = null)
    where T : class
{
    public string Volume { get; } = volume ?? AppDomain.CurrentDomain.BaseDirectory;

    public abstract T Create(string filename);

    public abstract void Save(string filename, T instance);
}

public class CookieManager : FileManager<ZipArchive>
{
    public override ZipArchive Create(string filename) =>
        throw new NotImplementedException();

    public override void Save(string filename, ZipArchive instance) =>
        throw new NotImplementedException();
}