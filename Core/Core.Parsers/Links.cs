using Core.Models;
using Core.Models.Extensions;
using Core.Parsers.Extensions;

namespace Core.Parsers;

public static class Links
{
    public static IEnumerable<Account> LinksFromLogs(this IEnumerable<string> links, string path)
        => Directory.GetDirectories(path)
            .SelectPerThread(links.LinksFromLog)
            .SelectMany(x => x);
    
    public static IEnumerable<Account> LinksFromLog(this IEnumerable<string> links, string path)
    {
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);
        
        var passwords = Path.Combine(path, "Passwords.txt");
            
        if (!File.Exists(passwords)) return Array.Empty<Account>();

        return passwords.ReadAccounts(urlPredicate: url => links.Any(url.Contains));
    }
}