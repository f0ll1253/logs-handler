using Core.Models;

namespace Core.Parsers.Extensions;

public static class StringExtensions
{
    public static IEnumerable<Account> ReadAccounts(
        this string filepath,
        Predicate<string>? urlPredicate = null, 
        Predicate<string>? usernamePredicate = null, 
        Predicate<string>? passwordPredicate = null,
        Action<Account>? func = null)
    {
        using var reader = new StreamReader(filepath);
        
        while (!reader.EndOfStream)
        {
            var account = reader.ReadAccount(urlPredicate, usernamePredicate, passwordPredicate);
                
            if (account is null) continue;
            
            func?.Invoke(account);
            
            yield return account;
        }
    }
    
    public static Account? ReadAccount(this StreamReader reader, 
        Predicate<string>? urlPredicate = null, 
        Predicate<string>? usernamePredicate = null, 
        Predicate<string>? passwordPredicate = null)
    {
        var line = reader.ReadLine();
                
        if (line is null || !line.StartsWith("URL")) return null;

        var url = line["URL: ".Length..line.Length];

        if (!urlPredicate?.Invoke(url) ?? false) return null;

        var username = reader.ReadLine()!;
        username = username["Username: ".Length..username.Length];
        
        if (!usernamePredicate?.Invoke(username) ?? false) return null;
                
        var password = reader.ReadLine()!;
        password = password["Password: ".Length..password.Length];
        
        if (!passwordPredicate?.Invoke(password) ?? false) return null;

        return new Account(url, username, password);
    }
}