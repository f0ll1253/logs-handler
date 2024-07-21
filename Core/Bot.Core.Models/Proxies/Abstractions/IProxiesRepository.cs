using Bot.Core.Models.Abstractions;

namespace Bot.Core.Models.Proxies.Abstractions {
	public interface IProxiesRepository<T> : IRepository<T, string> where T : class, IProxy;
}