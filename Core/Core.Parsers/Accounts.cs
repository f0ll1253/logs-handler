using Core.Models;
using Core.Parsers.Extensions;

namespace Core.Parsers;

public static class Accounts
{
    public static IDictionary<string, IEnumerable<Account>> AccountsFromLogs(this IEnumerable<string> domains, string path)
        => domains.ToDictionary(x => x, x => x.AccountsFromLogs(path));
    
    public static IDictionary<string, IEnumerable<Account>> AccountsFromLog(this IEnumerable<string> domains, string path)
        => domains.ToDictionary(x => x, x => x.AccountsFromLog(path));
    
    public static IEnumerable<Account> AccountsFromLogs(this string domain, string path)
        => Directory.GetDirectories(path)
            .SelectMany(domain.AccountsFromLog);
    
    public static IEnumerable<Account> AccountsFromLog(this string domain, string path)
    {
        var file = Path.Combine(path, "Passwords.txt");

        if (!File.Exists(file)) return ArraySegment<Account>.Empty;

        return file.ReadAccounts(urlPredicate: url => url.StartsWith($"https://{domain}"));
    }
}