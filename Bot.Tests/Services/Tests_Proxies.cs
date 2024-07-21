using System.Net;
using System.Reflection;

using Bot.Services.Proxies.Data;
using Bot.Services.Proxies.Models;
using Bot.Services.Proxies.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bot.Tests.Services {
	public class Tests_Proxies {
		private readonly ProxiesConfiguration _configuration;

		public Tests_Proxies() {
			var config = new ConfigurationBuilder()
						 .AddUserSecrets(Assembly.GetExecutingAssembly())
						 .AddInMemoryCollection()
						 .Build();

			_configuration = config.Get<ProxiesConfiguration>()!;
		}

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
				new NullLogger<Proxies>()
			);
		}

		#region Test Convertion

		[TestCase("1.1.1.1:80:username:password")]
		[TestCase("1.1.1.1:80@username:password")]
		[TestCase("username:password:1.1.1.1:80")]
		[TestCase("username:password@1.1.1.1:80")]
		public void Test_StrToProxy(string str) {
			var proxy = (Proxy)str;

			Assert.Multiple(
				() => {
					Assert.That(proxy.Host, Is.EqualTo("1.1.1.1"));
					Assert.That(proxy.Port, Is.EqualTo(80));
					Assert.That(proxy.Username, Is.EqualTo("username"));
					Assert.That(proxy.Password, Is.EqualTo("password"));

					Assert.That(proxy.Type, Is.EqualTo(ProxyType.Http));
					Assert.IsFalse(proxy.IsInUse);
				}
			);
		}

		[TestCase("1.1.1.1", 80, "username", "password")]
		public void Test_ProxyToStr(string host, int port, string username, string password) {
			var proxy = new Proxy {
				Host = host,
				Port = (uint)port,
				Username = username,
				Password = password
			};

			Assert.That((string)proxy, Is.EqualTo($"{host}:{port}:{username}:{password}"));
		}
		
		[TestCase("1.1.1.1", 80, "username", "password")]
		public void Test_ProxyToWebProxy(string host, int port, string username, string password) {
			var proxy = (WebProxy)new Proxy {
				Host = host,
				Port = (uint)port,
				Username = username,
				Password = password
			};

			var web_proxy = new WebProxy(new Uri($"http://{host}:{port}")) {
				Credentials = new NetworkCredential(username, password)
			};

			Assert.That(web_proxy.Address, Is.EqualTo(proxy.Address));

			Assert.Multiple(
				() => {
					var credentials = (NetworkCredential)proxy.Credentials;
					var web_credentials = (NetworkCredential)web_proxy.Credentials;

					Assert.NotNull(credentials);
					Assert.NotNull(web_credentials);

					Assert.That(credentials.UserName, Is.EqualTo(web_credentials.UserName));
					Assert.That(credentials.Password, Is.EqualTo(web_credentials.Password));
				}
			);
		}

		[Test]
		public void Test_InvalidConvertion() {
			foreach (var str in _configuration.Invalid) {
				Assert.Throws<ArgumentException>(
					() => {
						var proxy = (Proxy)str;
					},
					$"Invalid string to proxy convertion: {str}"
				);
			}
		}

		#endregion
	}

	public class ProxiesConfiguration {
		public ICollection<string> Valid { get; set; } = [];
		public ICollection<string> Invalid { get; set; } = [];
	}
}