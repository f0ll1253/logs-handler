using Core.Models;
using Core.Models.Extensions;
using Core.Parsers.Extensions;

namespace Core.Parsers;

public static class Requests
{
    public static IEnumerable<Account> RequestsFromLogs(this IEnumerable<string> links, string path) =>
        Directory.GetDirectories(path)
                 .SelectPerTask(links.RequestsFromLog)
                 .SelectMany(x => x)
                 .Where(x => x is { })
                 .DistinctBy(x =>
                 {
                     int domainend = x!.Url.IndexOf('\\', "https:\\".Length);

                     return x.Username + x.Password +
                            x.Url["https:\\".Length..(domainend == -1 ? x.Url.Length : domainend)];
                 })!;

    public static IEnumerable<Account?> RequestsFromLog(this IEnumerable<string> links, string path)
    {
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

        string passwords = Path.Combine(path, "Passwords.txt");

        if (!File.Exists(passwords)) return Array.Empty<Account>();

        return passwords.ReadAccounts(url => links.Any(url.Contains));
    }
}