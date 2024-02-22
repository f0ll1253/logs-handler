using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Core.Wallets.Crypting;

public static class AesGcmCryptor
{
    public static string? DecryptVault(string password, Vault vault)
    {
        byte[] decrypted;

        byte[] key = AesGcmCryptor.KeyFromPassword(password, vault.Salt);
        byte[] data = Convert.FromBase64String(vault.Data);
        byte[] iv = Convert.FromBase64String(vault.Iv);

        GcmBlockCipher cipher = new(new AesEngine());
        ICipherParameters parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);

        try
        {
            cipher.Init(false, parameters);
            decrypted = new byte[cipher.GetOutputSize(data.Length)];
            int retLen = cipher.ProcessBytes(data, 0, data.Length, decrypted, 0);
            cipher.DoFinal(decrypted, retLen);
        }
        catch
        {
            return null;
        }

        return Encoding.UTF8.GetString(decrypted);
    }

    private static byte[] KeyFromPassword(string password, string salt)
    {
        byte[] derivedKey;

        byte[] bufPassword = Encoding.UTF8.GetBytes(password);
        byte[] bufSalt = Convert.FromBase64String(salt);

        using (var pbkdf2 = new Rfc2898DeriveBytes(
                   bufPassword,
                   bufSalt,
                   10000,
                   HashAlgorithmName.SHA256))
        {
            derivedKey = pbkdf2.GetBytes(32);
        }

        return derivedKey;
    }
}