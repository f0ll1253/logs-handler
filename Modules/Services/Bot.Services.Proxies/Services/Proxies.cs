using Bot.Services.Proxies.Data;
using Bot.Services.Proxies.Models;
using Bot.Services.Proxies.Models.Base;

using Microsoft.Extensions.Logging;

namespace Bot.Services.Proxies.Services {
	public class Proxies(ProxiesDbContext context, ILogger<Proxies> logger) : BaseProxiesRepository<Proxy>(context, logger);
}