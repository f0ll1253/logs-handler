using System.Reflection;

using Bot.Services.Proxies.Data;
using Bot.Services.Proxies.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Tests.Services {
	public class Tests_Proxies {
		private readonly LoggerFactory _logger_factory = new();
		private readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
															 .AddUserSecrets(Assembly.GetExecutingAssembly())
															 .AddInMemoryCollection()
															 .Build();

		private ProxiesDbContext _context;
		private Proxies _proxies;
		
		[OneTimeSetUp]
		public void OneTimeSetUp() {
			#region Context

			var options_builder = new DbContextOptionsBuilder<ProxiesDbContext>();

			options_builder.UseInMemoryDatabase("Proxies");

			_context = new(options_builder.Options);

			#endregion

			_proxies = new(
				_context,
				new() {
					MaxThreads = 10,
					OnInCheck = true,
					OnOutCheck = true,
				},
				_logger_factory.CreateLogger<Proxies>()
			);
		}

		#region Test Convertion

		[Test]
		public void Test_StrToProxy() {
			throw new NotImplementedException();
		}

		[Test]
		public void Test_ProxyToStr() {
			throw new NotImplementedException();
		}

		[Test]
		public void Test_ProxyToWebProxy() {
			throw new NotImplementedException();
		}

		[Test]
		public void Test_InvalidConvertion() {
			throw new NotImplementedException();
		}

		#endregion
	}
}