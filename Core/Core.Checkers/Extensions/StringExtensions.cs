using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Core.Checkers.Extensions;

internal static class StringExtensions
{
    public static string GetMetaValue(this string str, string name = "csrf-token", string contentname = "content")
    {
        var html = new HtmlDocument();
        
        html.LoadHtml(str);
        
        return html.QuerySelector($"meta[name='{name}']")!.Attributes[contentname].Value;
    }

    public static Task<string> FindRegex(this string str, Regex regex) => Task.FromResult(regex.Match(str).Groups[1].Value);
}