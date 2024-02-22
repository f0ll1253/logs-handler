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
    private readonly List<Proxy> _proxies = [];
    private readonly int _timeout;
    private readonly ProxyType _type;
    private int _last;

    public ProxyPool(int timeout = 5000, ProxyType type = ProxyType.Http)
    {
        this._timeout = timeout;
        this._type = type;
    }

    public void Clear()
    {
        this._proxies.Clear();
        this._last = 0;
    }

    public async Task LoadAsync(string file)
    {
        if (!File.Exists(file)) throw new Exception("Proxies file doesn't exists");

        using var reader = new StreamReader(file);

        Clear();

        while (!reader.EndOfStream)
        {
            string[]? args = (await reader.ReadLineAsync())?.Replace('@', ':').Split(':');

            if (args is not { Length: >= 2 }) continue;

            switch (args.Length)
            {
                case 2:
                    Add(new Proxy(args[0], int.Parse(args[1])));
                    break;
                case 4:
                    Add(new Proxy(args[2], int.Parse(args[3]), args[0], args[1]));
                    break;
            }
        }
    }

    public void AddRange(IEnumerable<Proxy> arr)
    {
        foreach (var proxy in arr) Add(proxy);
    }

    public bool Add(Proxy proxy)
    {
        if (string.IsNullOrEmpty(proxy.Host) || proxy.Port <= 0) return false;

        this._proxies.Add(proxy);

        return true;
    }

    public async Task<HttpClient> TakeClient(AuthenticationHeaderValue? authorization = null) =>
        new(await TakeHandler(), true)
        {
            DefaultRequestHeaders =
            {
                Authorization = authorization
            }
        };

    public async Task<HttpClientHandler> TakeHandler() =>
        new()
        {
            Proxy = await TakeWebProxy(),
            UseProxy = true,
            AllowAutoRedirect = false,
            SslProtocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13
        };

    public async Task<string> TakeProxyString() =>
        (await _TakeProxy()).ToString();

    public async Task<WebProxy> TakeWebProxy()
    {
        var proxy = await _TakeProxy();

        return _CreateWebProxy(proxy);
    }

    private async Task<Proxy> _TakeProxy()
    {
        if (this._proxies.Count == 0) throw new Exception("Proxies not loaded");

        if (this._last >= this._proxies.Count) this._last = 0;

        if (this._proxies.Count == 1) return this._proxies.First();

        Proxy? proxy = null;

        do
        {
            if (proxy is { }) this._proxies.Remove(proxy);

            proxy = this._proxies[this._last];
        } while (!await _ProxyAvailable(proxy));

        this._last++;

        return proxy;
    }

    private WebProxy _CreateWebProxy(Proxy proxy) =>
        new()
        {
            Address = new Uri($"{_GetProtocol()}://{proxy.Host}:{proxy.Port}"),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,

            Credentials = new NetworkCredential(proxy.Login, proxy.Password)
        };

    private async Task<bool> _ProxyAvailable(Proxy proxy)
    {
        using var http = new HttpClient(new HttpClientHandler
        {
            Proxy = new WebProxy
            {
                Address = new Uri($"{_GetProtocol()}://{proxy.Host}:{proxy.Port}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,

                Credentials = new NetworkCredential(proxy.Login, proxy.Password)
            },
            UseProxy = true,
            AllowAutoRedirect = false,
            SslProtocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13
        }, true);

        http.Timeout = TimeSpan.FromMilliseconds(this._timeout);

        try
        {
            await http.GetAsync("http://ip-api.com/line/?fields=8192");
        }
        catch
        {
            return false;
        }

        return true;
    }

    private string _GetProtocol() =>
        this._type switch
        {
            ProxyType.Http  => "http",
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
    public override string ToString() =>
        $"{(string.IsNullOrEmpty(Login) ? "" : $"{Login}:{Password}@")}{Host}:{Port}";
}