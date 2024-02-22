using System.Text.RegularExpressions;
using Core.Models;

namespace Core.Parsers.Extensions;

internal static class StringExtensions
{
    private static readonly Regex _password = new("^(?=.*[0-9].*[0-9])(?=.*[a-z].*[a-z].*[a-z]).{8}$");

    public static IEnumerable<Account> ReadAccounts(
        this string filepath,
        Predicate<string>? urlPredicate = null,
        Predicate<string>? usernamePredicate = null,
        Predicate<string>? passwordPredicate = null)
    {
        using var reader = new StreamReader(filepath);

        while (!reader.EndOfStream)
        {
            var account = reader.ReadAccount(Directory.GetParent(filepath)!.FullName, urlPredicate, usernamePredicate,
                passwordPredicate);

            if (account is null) continue;

            yield return account;
        }
    }

    public static Account? ReadAccount(
        this StreamReader reader,
        string logpath = "",
        Predicate<string>? urlPredicate = null,
        Predicate<string>? usernamePredicate = null,
        Predicate<string>? passwordPredicate = null)
    {
        string? line = reader.ReadLine();

        if (line is null || !line.StartsWith("URL")) return null;

        string? url, username, password;

        try
        {
            url = line["URL: ".Length..line.Length];

            if (!urlPredicate?.Invoke(url) ?? false) return null;
        }
        catch
        {
            return null;
        }

        username = reader.ReadLine();

        try
        {
            username = username?[(username.IndexOf(' ') + 1)..username.Length];

            if (username is null || (!usernamePredicate?.Invoke(username) ?? false) || username == "UNKNOWN")
                return null;
        }
        catch
        {
            return null;
        }

        password = reader.ReadLine();

        try
        {
            password = password?[(password.IndexOf(' ') + 1)..password.Length];

            if (password is null || !_password.IsMatch(password) ||
                (!passwordPredicate?.Invoke(password) ?? false)) return null;
        }
        catch
        {
            return null;
        }

        return new Account(username, password, url, logpath);
    }
}