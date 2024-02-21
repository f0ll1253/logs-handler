using Core.Models;
using Core.Models.Extensions;
using Core.Parsers.Extensions;

namespace Core.Parsers;

public static class Links
{
    public static IEnumerable<Account> LinksFromLogs(this IEnumerable<string> links, string path)
        => Directory.GetDirectories(path)
            .SelectPerTask(links.LinksFromLog)
            .SelectMany(x => x)
            .Where(x => x is not null)
            .DistinctBy(x =>
            {
                var domainend = x!.Url.IndexOf('\\', "https:\\".Length);
                
                return x.Username + x.Password + x.Url["https:\\".Length..(domainend == -1 ? x.Url.Length : domainend)];
            })!;
    
    public static IEnumerable<Account?> LinksFromLog(this IEnumerable<string> links, string path)
    {
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);
        
        var passwords = Path.Combine(path, "Passwords.txt");
            
        if (!File.Exists(passwords)) return Array.Empty<Account>();

        return passwords.ReadAccounts(urlPredicate: url => links.Any(url.Contains));
    }
}