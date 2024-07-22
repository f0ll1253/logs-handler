using System.Net;

using Bot.Core.Models;
using Bot.Core.Models.Base;
using Bot.Core.Models.Proxies.Abstractions;
using Bot.Services.Proxies.Configuration;
using Bot.Services.Proxies.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bot.Services.Proxies.Models.Base {
	public abstract class BaseProxiesRepository<T>(ProxiesDbContext context, ILogger? logger) : BaseRepository<T, string>(context, logger), IProxiesRepository<T> where T : Proxy {

		public ProxyConfiguration Config { get; init; } = new();
		
		public override async Task<bool> AddAsync(T proxy) {
			if (Config.OnInCheck && !await _CheckProxyAsync(proxy)) {
				logger?.LogWarning("[Proxies] Proxy is not valid: {proxy}", (string)proxy);
				
				return false;
			}

			return await _TrySaveAsync(context.AddAsync(proxy).AsTask());
		}

		public override async Task<bool> AddRangeAsync(ICollection<T> proxies) {
			if (Config.OnInCheck) {
				var result = new List<T>();

				proxies.WithThreads(
					async proxy => {
						if (await _CheckProxyAsync(proxy)) {
							result.Add(proxy);
						}
					},
					Config.MaxThreads
				);

				await context.AddRangeAsync(result);
			}
			else {
				await context.AddRangeAsync(proxies);
			}

			return await _TrySaveAsync();
		}

		public override async Task<T?> GetAsync(string key) {
			var proxy = await base.GetAsync(key);

			if (Config.OnOutCheck && proxy is { } && !await _CheckProxyAsync(proxy)) {
				logger?.LogWarning("[Proxies] Proxy is not valid: {proxy}", (string)proxy);

				await _TrySaveAsync(() => context.Remove(proxy));
				
				return null;
			}
            
			return proxy;
		}

		public virtual async IAsyncEnumerable<T> TakeAsync(int count) {
			var proxies = context.Set<T>().OrderBy(x => x.Index);
			
			ICollection<T> result;

			if (Config.OnOutCheck) {
				result = new List<T>();

				proxies.WithThreads(
					async proxy => {
						if (result.Count >= count) {
							return;
						}
						
						if (await _CheckProxyAsync(proxy)) {
							result.Add(proxy);
						}
						
						// TODO else set proxy is not working
					},
					Config.MaxThreads,
					_ => result.Count >= count
				);
			}
			else {
				result = await proxies.Take(count).ToListAsync();
			}

			await context.SaveChangesAsync();

			foreach (var proxy in result.Take(count)) {
				yield return proxy;
			}
		}
		
		// Protected
		protected async Task<bool> _CheckProxyAsync(T proxy) {
			var handler = new HttpClientHandler {
				UseCookies = false,
				UseProxy = true,
				Proxy = (WebProxy)proxy
			};

			using (var http = new HttpClient(handler, true)) {
				http.Timeout = TimeSpan.FromMilliseconds(Config.CheckTimeout);
				
				HttpResponseMessage response;
				
				try {
					response = await http.GetAsync(Config.CheckUrl);
				} catch {
					return false;
				}

				if (!response.IsSuccessStatusCode) {
					return false;
				}
			}

			return true;
		}
	}
}