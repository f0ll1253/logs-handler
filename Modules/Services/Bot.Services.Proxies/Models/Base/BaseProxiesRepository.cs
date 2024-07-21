using System.Net;

using Bot.Core.Models;
using Bot.Core.Models.Base;
using Bot.Core.Models.Proxies.Abstractions;
using Bot.Services.Proxies.Configuration;
using Bot.Services.Proxies.Data;

using Microsoft.Extensions.Logging;

namespace Bot.Services.Proxies.Models.Base {
	public abstract class BaseProxiesRepository<T>(ProxiesDbContext context, ProxyConfiguration config, ILogger? logger) : BaseRepository<T, string>(context, logger), IProxiesRepository<T> where T : Proxy {
		protected static readonly AutoResetEvent _event = new(false);
		
		public override async Task<bool> AddAsync(T proxy) {
			if (config.OnInCheck && !await _CheckProxyAsync(proxy)) {
				logger?.LogWarning("[Proxies] Proxy is not valid: {proxy}", (string)proxy);
				
				return false;
			}

			return await _TrySaveAsync(context.AddAsync(proxy).AsTask());
		}

		public override Task<bool> AddRangeAsync(ICollection<T> proxies) {
			if (config.OnInCheck) {
				foreach (var group in proxies.GroupBy(config.MaxThreads)) {
					_event.Reset();

					for (int threads = group.Count(), i = 0; i < group.Count(); i++) {
						var proxy = group.ElementAt(i);
						
						new Thread(
							async () => {
								if (await _CheckProxyAsync(proxy)) {
									await context.AddAsync(proxy);
								}

								if (Interlocked.Decrement(ref threads) == 0) {
									_event.Set();
								}
							}
						).Start();
					}

					_event.WaitOne();
				}
			}

			return _TrySaveAsync();
		}

		public override async Task<T?> GetAsync(string key) {
			var proxy = await base.GetAsync(key);

			if (config.OnOutCheck && proxy is { } && !await _CheckProxyAsync(proxy)) {
				logger?.LogWarning("[Proxies] Proxy is not valid: {proxy}", (string)proxy);

				await _TrySaveAsync(() => context.Remove(proxy));
				
				return null;
			}
            
			return proxy;
		}

		public virtual async IAsyncEnumerable<T> TakeAsync(int count) {
			var proxies = context.Set<T>()
								 .OrderBy(x => x.Index);
			var result = new List<T>();

			if (config.OnOutCheck) {
				foreach (var group in proxies.GroupBy(config.MaxThreads)) {
					_event.Reset();

					for (int threads = group.Count(), i = 0; i < group.Count(); i++ ) {
						var proxy = group.ElementAt(i);
						proxy.IsInUse = true;
					
						new Thread(
							async () => {
								if (result.Count < count && await _CheckProxyAsync(proxy)) {
									result.Add(proxy);
								}

								if (Interlocked.Decrement(ref threads) == 0) {
									_event.Set();
								}
							}
						).Start();
					}

					_event.WaitOne();
				}
			}

			await context.SaveChangesAsync();

			foreach (var proxy in result) {
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
				http.Timeout = TimeSpan.FromMilliseconds(config.CheckTimeout);
				
				HttpResponseMessage response;
				
				try {
					response = await http.GetAsync(config.CheckUrl);
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