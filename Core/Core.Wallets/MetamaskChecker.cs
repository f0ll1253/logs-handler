using BscScanner;
using Nethereum.Util;
using Nethereum.Web3;

namespace Core.Wallets;

public class MetamaskChecker
{
    private readonly string _bsc;
    private readonly string _eth;
    private readonly IReadOnlyDictionary<string, string> _networks;

    public MetamaskChecker(string eth, string bsc)
    {
        this._eth = eth;
        this._bsc = bsc;

        this._networks = new Dictionary<string, string>
        {
            { "Ethereum", $"https://mainnet.infura.io/v3/{this._eth}" },
            { "Linea", $"https://linea-mainnet.infura.io/v3/{this._eth}" },
            { "Polygon", $"https://polygon-mainnet.infura.io/v3/{this._eth}" }
        };
    }

    public async Task<Dictionary<string, BigDecimal>> Balance(string address)
    {
        var bsc = new BscScanClient(this._bsc);

        return await Switcher<KeyValuePair<string, BigDecimal>>(async (web, name) =>
                     {
                         var balance = await web.Eth.GetBalance.SendRequestAsync(address);

                         return KeyValuePair.Create(name,
                             Web3.Convert.FromWeiToBigDecimal(balance.Value,
                                 UnitConversion.EthUnit.Tether));
                     })
                     .Append(new KeyValuePair<string, BigDecimal>("BSC",
                         await bsc.GetBnbBalanceSingleAsync(address) *
                         Math.Pow(10, -18)))
                     .ToDictionaryAsync(x => x.Key, x => x.Value);
    }

    private async IAsyncEnumerable<T> Switcher<T>(Func<Web3, string, Task<T>> action)
    {
        foreach (var network in this._networks) yield return await action.Invoke(new Web3(network.Value), network.Key);
    }
}