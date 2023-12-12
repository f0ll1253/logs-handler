using CG.Web.MegaApiClient;
using Core.Models.Configs;
using Newtonsoft.Json;

namespace TelegramBot.Extensions;

public static class MegaApiClientExtensions
{
    public static MegaApiClient.LogonSessionToken LoginByCredentials(this IMegaApiClient client, MegaConfig config)
    {
        string? mfa;
            
        do
        {
            Console.Clear();
            Console.WriteLine("Write mfa code for mega");
            mfa = Console.ReadLine();
        } while (mfa is null);

        return client.Login(config.Login, config.Password, mfa);
    }

    public static void LoginBySession(this IMegaApiClient client, string path)
    {
        var session = JsonConvert.DeserializeObject<MegaApiClient.LogonSessionToken>(File.ReadAllText(path));

        client.Login(session);
    }
}