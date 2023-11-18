using Core.Models;
using Core.Parsers.Extensions;

namespace Core.Parsers;

public static class Links
{
    public static IEnumerable<Account> FromLogs(this string[] links, string path)
        => Directory.GetDirectories(path)
            .SelectMany(links.FromLog);
    
    public static IEnumerable<Account> FromLog(this string[] links, string path)
    {
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);
        
        var passwords = Path.Combine(path, "Passwords.txt");
            
        if (!File.Exists(passwords)) return Array.Empty<Account>();

        return passwords.ReadAccounts(urlPredicate: url => links.Any(url.Contains));
    }
}