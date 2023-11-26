using System.Net;
using System.Text;
using Core.Models;
using Newtonsoft.Json;

namespace Core.IGV;

public class IGVChecker
{
    private readonly ProxyPool _proxy;

    public IGVChecker(ProxyPool proxy)
    {
        _proxy = proxy;
    }
    
    public async Task<string?> TryLoginAsync(string login, string password)
    {
        using var http = await _proxy.TakeClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://paas-gateway.imetastore.io/account/oauth/token?client_id=default&client_secret=SADFAS345ASREGNREF");
        
        request.Content = new StringContent(JsonConvert.SerializeObject(new
        {
            grantType = "password",
            tenantId = "3332001",
            userType = 0,
            userName = login,
            password
        }), Encoding.UTF8, "application/json");

        var response = await http.SendAsync(request);

        if (response.StatusCode != (HttpStatusCode) 200) return null;

        var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())!;

        if ((string) json["code"] != "0000") return null;
        
        return json["data"]["access_token"];
    }
    
    // todo add items getter
}