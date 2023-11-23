using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;

namespace Core.Models;

public enum ProxyType
{
    Http,
    Https,
    Socks4,
    Socks5
}

public class ProxyPool
{
    private readonly int _timeout;
    private readonly ProxyType _type;
    private readonly List<Proxy> _proxies = new ();
    private int _last = 0;

    public ProxyPool(string validpath = "Proxies/valid.txt", string invalidpath = "Proxies/invalid.txt", int timeout = 5000, ProxyType type = ProxyType.Http)
    {
        _timeout = timeout;
        _type = type;
    }

    public Task<bool> TryAdd(string host, int port) => TryAdd(new Proxy(host, port));
    
    public Task<bool> TryAdd(string host, int port, string login, string password) => TryAdd(new Proxy(host, port, login, password));
    
    public async Task<bool> TryAdd(Proxy proxy, StreamWriter? valid = null, StreamWriter? invalid = null)
    {
        if (string.IsNullOrEmpty(proxy.Host) || proxy.Port <= 0) return false;
        
        using var http = new HttpClient(new HttpClientHandler
        {
            Proxy = new WebProxy
            {
                Address = new Uri($"{_GetProtocol()}://{(string.IsNullOrEmpty(proxy.Login) ? "" : $"{proxy.Login}:{proxy.Password}@")}{proxy.Host}:{proxy.Port}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
            
                Credentials = new NetworkCredential(proxy.Login, proxy.Password)
            },
            UseProxy = true,
            AllowAutoRedirect = false,
            SslProtocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13,
        }, true)
        {
            Timeout = TimeSpan.FromMilliseconds(_timeout)
        };
        
        try
        {
            await http.GetAsync("http://ip-api.com/line/?fields=8192");
        }
        catch
        {
            invalid?.WriteLine(proxy);
            
            return false;
        }

        valid?.WriteLine(proxy);
        _proxies.Add(proxy);
        
        return true;
    }

    public HttpClient TakeClient(AuthenticationHeaderValue? authorization = null) 
        => new(TakeHandler(), true)
    {
        DefaultRequestHeaders =
        {
            Authorization = authorization
        }
    };

    public HttpClientHandler TakeHandler() 
        => new()
    {
        Proxy = TakeProxy(),
        UseProxy = true,
        AllowAutoRedirect = false,
        SslProtocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13
    };

    public WebProxy TakeProxy()
    {
        if (_proxies.Count == 0) throw new Exception("Proxies not loaded");
        
        if (_last >= _proxies.Count) _last = 0;

        var proxy = _proxies[_last];
        _last++;

        return new WebProxy
        {
            Address = new Uri($"{_GetProtocol()}://{proxy.Host}:{proxy.Port}"),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,
            
            Credentials = new NetworkCredential(proxy.Login, proxy.Password)
        };
    }

    private string _GetProtocol() => _type switch
    {
        ProxyType.Http => "http",
        ProxyType.Https => "https"
        // todo add other proxy types
    };
}

public record Proxy(
    string Host,
    int Port,
    string Login = "",
    string Password = ""
)
{
    public override string ToString() => $"{(string.IsNullOrEmpty(Login) ? "" : $"{Login}:{Password}@")}{Host}:{Port}";
}