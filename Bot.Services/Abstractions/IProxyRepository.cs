using Bot.Models.Proxies;

namespace Bot.Services.Abstractions {
	public interface IProxyRepository {
		Task<Proxy> GetAsync();
		Task CloseAsync(string id);
	}
}