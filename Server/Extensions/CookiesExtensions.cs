namespace Server.Extensions;

public static class CookiesExtensions
{
    public static IEnumerable<FileInfo> GetCookieFiles(this string log) =>
        Directory.GetFiles(Path.Combine(log, "Cookies"))
                 .Select(x => new FileInfo(x));
}