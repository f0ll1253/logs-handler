using System.Text.RegularExpressions;
using Core.Models;
using Core.Models.Extensions;
using Core.Parsers.Extensions;

namespace Core.Parsers;

public static class Accounts
{
    public static IDictionary<string, IEnumerable<Account>> AccountsFromLogs(this IEnumerable<string> domains, string path)
        => Directory.GetDirectories(path)
            .SelectPerThread(domains.AccountsFromLog)
            .SelectMany(x => x)
            .ToLookup(x => x.Key)
            .ToDictionary(x => x.Key, x => x.SelectMany(x => x.Value));

    private static readonly Regex _domain = new ("https://(.*?)/");
    public static IDictionary<string, IEnumerable<Account>> AccountsFromLog(this IEnumerable<string> domains, string path)
    {
        var file = Path.Combine(path, "Passwords.txt");

        if (!File.Exists(file)) return new Dictionary<string, IEnumerable<Account>>();

        var result = new Dictionary<string, List<Account>>();

        foreach (var account in file.ReadAccounts())
        {
            var domain = _domain.Match(account.Url);

            if (!domain.Success 
                || domains.FirstOrDefault(x => domain.Groups[1].Value.Contains(x)) is not {} picked 
                || string.IsNullOrEmpty(account.Username) 
                || string.IsNullOrEmpty(account.Password)) continue;
            
            if (result.TryGetValue(picked, out var list))
                list.Add(account);
            else
                result.Add(picked, new List<Account> {account});
        }

        return result.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
    }
    
    public static IEnumerable<Account> AccountsFromLogs(this string domain, string path)
        => Directory.GetDirectories(path)
            .SelectMany(domain.AccountsFromLog);
    
    public static IEnumerable<Account> AccountsFromLog(this string domain, string path)
    {
        var file = Path.Combine(path, "Passwords.txt");

        if (!File.Exists(file)) return ArraySegment<Account>.Empty;

        return file.ReadAccounts(urlPredicate: url => new Uri(url).Host.Contains(domain));
    }
}