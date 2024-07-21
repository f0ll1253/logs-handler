using System.Net;

using Bot.Core.Models;
using Bot.Core.Models.Exceptions;
using Bot.Core.Models.Proxies.Abstractions;
using Bot.Services.Proxies.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Services.Proxies.Models.Base {
	public abstract class BaseProxiesRepository<T>(ProxiesDbContext context, IConfiguration config, ILogger<BaseProxiesRepository<T>> logger) : IProxiesRepository<T> where T : Proxy {
		protected static readonly AutoResetEvent _event = new(false);
		
		public virtual async Task<bool> AddAsync(T proxy) {
			if (!await _CheckProxyAsync(proxy)) {
				logger.LogWarning("[Proxies] Proxy is not valid: {proxy}", (string)proxy);
				
				return false;
			}

			return await _TrySaveAsync(context.AddAsync(proxy).AsTask());
		}

		public virtual Task<bool> AddRangeAsync(ICollection<T> proxies) {
			int count;
			
			foreach (var group in proxies.GroupBy(int.Parse(config["Multithreading:Proxy:MaxThreads"]))) {
				_event.Reset();

				count = group.Count();

				foreach (var proxy in group) {
					new Thread(
						async () => {
							if (await _CheckProxyAsync(proxy)) {
								await context.AddAsync(proxy);
							}

							if (Interlocked.Decrement(ref count) == 0) {
								_event.Set();
							}
						}
					).Start();
				}

				_event.WaitOne();
			}

			return _TrySaveAsync();
		}

		public virtual async Task<bool> RemoveAsync(string key) {
			if (await context.FindAsync<T>(key) is not { } entity) {
				return false;
			}

			return await _TrySaveAsync(() => context.Remove(entity));
		}

		public virtual Task<bool> UpdateAsync(T proxy) => _TrySaveAsync(() => context.Update(proxy));

		public virtual Task<T?> GetAsync(string key) => context.FindAsync<T>(key).AsTask();

		protected async Task<bool> _CheckProxyAsync(T proxy) {
			var handler = new HttpClientHandler {
				UseCookies = false,
				UseProxy = true,
				Proxy = (WebProxy)proxy
			};

			using (var http = new HttpClient(handler, true)) {
				http.Timeout = TimeSpan.FromMilliseconds(uint.Parse(config["Proxy:CheckTimeout"]));
				
				HttpResponseMessage response;
				
				try {
					response = await http.GetAsync(config["Proxy:CheckUrl"]);
				} catch {
					return false;
				}

				if (!response.IsSuccessStatusCode) {
					return false;
				}
			}

			return true;
		}

		protected Task<bool> _TrySaveAsync(Action action) => _TrySaveAsync(Task.Run(action));
		
		protected async Task<bool> _TrySaveAsync(Task? action = null) {
			if (action is not null) {
				await action;
			}
			
			try {
				await context.SaveChangesAsync();
			} catch (Exception e) {
				logger.LogError(new ContextSaveException(e, "Proxies"), null);

				context.ChangeTracker.Clear();
                
				return false;
			}

			return true;
		}
	}
}