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
		private ProxiesConfiguration _configuration;
		private ProxiesDbContext _context;
		private Proxies _proxies;
		
		[OneTimeSetUp]
		public void OneTimeSetUp() {
			_configuration = "Proxies".GetConfiguration<ProxiesConfiguration>();
			
			#region Context

			var options_builder = new DbContextOptionsBuilder<ProxiesDbContext>();

			options_builder.UseInMemoryDatabase("Proxies");

			_context = new(options_builder.Options);

			#endregion

			_proxies = new(_context, new NullLogger<Proxies>()) {
				Config = new() {
					MaxThreads = 10,
					OnInCheck = true,
					OnOutCheck = true,
					CheckTimeout = 10_000
				}
			};
		}

		#region Test Convertion

		[TestCase("1.1.1.1:80:username:password")]
		[TestCase("1.1.1.1:80@username:password")]
		[TestCase("username:password:1.1.1.1:80")]
		[TestCase("username:password@1.1.1.1:80")]
		[Order(-1)]
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
			foreach (var str in _configuration.Invalid.ToArray()) {
				Assert.Throws<ArgumentException>(
					() => {
						var proxy = (Proxy)str;
					},
					$"Invalid string to proxy convertion: {str}"
				);
			}
		}

		#endregion

		#region Test Adding

		[Test, Order(0)]
		public async Task Test_AddAsync() {
			foreach (var str in _configuration.Valid.ToArray()) {
				Assert.That(await _proxies.AddAsync(str), Is.True);
			}
		}

		[Test, Order(0)]
		public async Task Test_AddRangeAsync() {
			Assert.That(await _proxies.AddRangeAsync(_configuration.Valid.ToArray().Select(x => (Proxy)x).ToList()), Is.True);
		}

		#endregion

		#region Test Getting

		[Test, Order(1)]
		public async Task Test_GetAsync() {
			if (await _context.Proxies.FirstOrDefaultAsync() is not {} proxy) {
				throw new ArgumentException("Proxy not found");
			}
			
			Assert.That(await _proxies.GetAsync(proxy.Id), Is.EqualTo(proxy));
		}

		[TestCase(10), Order(1)]
		public async Task Test_TakeAsync(int count) {
			var from_context = await _context.Proxies.Take(count).OrderBy(x => x.Index).ToArrayAsync();
			var from_repository = await _proxies.TakeAsync(count).ToArrayAsync();

			if (from_repository.Length != count) {
				throw new("Count of elements more or less than needed");
			}

			for (var i = 0; i < count; i++) {
				Assert.IsTrue(from_context.Contains(from_repository[i]));
			}
		}

		#endregion

		[TestCase(20), Order(1)]
		public async Task Test_AfterAddCount(int expected_count) {
			Assert.That(await _context.Proxies.CountAsync(), Is.EqualTo(expected_count));
		}

		#region Other

		[TestCase("0.0.0.0", 80, "username", "password"), Order(2)]
		public async Task Test_UpdateAsync(string host, int port, string username, string password) {
			var proxy = await _context.Proxies.FirstAsync();

			proxy.Host = host;
			proxy.Port = (uint)port;
			proxy.Username = username;
			proxy.Password = password;
			
			Assert.That(await _proxies.UpdateAsync(proxy), Is.True);
		}
		
		[Test, Order(3)]
		public async Task Test_RemoveAsync() {
			Assert.That(await _proxies.RemoveAsync(await _context.Proxies.Select(x => x.Id).FirstAsync()), Is.True);
		}

		#endregion
	}

	public class ProxiesConfiguration {
		public ICollection<string> Valid { get; set; } = [];
		public ICollection<string> Invalid { get; set; } = [];
	}
}