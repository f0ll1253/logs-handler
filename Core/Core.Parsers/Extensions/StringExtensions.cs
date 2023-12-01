using Core.Models;

namespace Core.Parsers.Extensions;

public static class StringExtensions
{
    public static IEnumerable<Account> ReadAccounts(
        this string filepath,
        Predicate<string>? urlPredicate = null, 
        Predicate<string>? usernamePredicate = null, 
        Predicate<string>? passwordPredicate = null)
    {
        using var reader = new StreamReader(filepath);
        
        while (!reader.EndOfStream)
        {
            var account = reader.ReadAccount(urlPredicate, usernamePredicate, passwordPredicate);
                
            if (account is null) continue;
            
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

        string? url, username, password;
        
        try
        {
            url = line["URL: ".Length..line.Length];
        }
        catch
        {
            return null;
        }

        if (!urlPredicate?.Invoke(url) ?? false) return null;

        username = reader.ReadLine();

        try
        {
            username = username?["Username: ".Length..username.Length];
        }
        catch
        {
            return null;
        }
        
        if (username is null || (!usernamePredicate?.Invoke(username) ?? false) || username == "UNKNOWN") return null;
                
        password = reader.ReadLine();

        try
        {
            password = password?["Password: ".Length..password.Length];
        }
        catch
        {
            return null;
        }
        
        if (password is null || (!passwordPredicate?.Invoke(password) ?? false)) return null;

        return new Account(username, password, url);
    }
}