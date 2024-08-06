using Bot.Services.Database.Data;
using Bot.Services.Database.Models;
using Bot.Services.Database.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Newtonsoft.Json;

using Thinktecture;
using Thinktecture.EntityFrameworkCore.BulkOperations;

namespace Bot.Tests.Services {
	public class Tests_Database {
		
		private DatabaseConfiguration _configuration;

		private DatabaseDbContext _context;
		private CredentialsRepository _credentials;
		
		[OneTimeSetUp]
		public void OneTimeSetUp() {
			_configuration = "Database".GetConfiguration<DatabaseConfiguration>();
			
			#region Context

			var options_builder = new DbContextOptionsBuilder<DatabaseDbContext>();

			options_builder.UseSqlServer("Server=localhost;Database=Database;User Id=sa;Password=Password1!;TrustServerCertificate=True", options => {
					options.AddBulkOperationSupport();
				}
			);

			_context = new(options_builder.Options);

			_context.Database.EnsureCreated();
			_context.Database.SetCommandTimeout(TimeSpan.FromMinutes(1));

			#endregion

			_credentials = new(_context, new NullLogger<CredentialsRepository>());
		}
		
		// global
		[TestCase("https://sso.hhs.state.ma.us/oam/server/obrareq.cgi:mgrogan8:Maryed0551!", false, "https", "ma.us", "mgrogan8", "Maryed0551!")]
		[TestCase("https://id.n-able.com/identity/login:mike@truebluecomputers.com:Maryed12232412!!!!", false, "https", "n-able.com", "mike@truebluecomputers.com", "Maryed12232412!!!!")]
		[TestCase("https://olo.friendlys.mymicros.net/TemplateUI2.0/en-us/Friendlys/FIC/Ludlow-CenterSt/Account/LogIn:mikergrogan@gmail.com:Maryed0551!", false, "https", "mymicros.net", "mikergrogan@gmail.com", "Maryed0551!")]
		[TestCase("android://MXo9ApqhUpzvVMVTAuOhshLS-7evRQw620c6-koe6AZOBVhbH6_gxBobPMY3qt8Fy5GKBGjGrPKRMBmYHXiA_g==@com.metlifeapps.metlifeus/:lucinda201212@gmail.com:lucindale8", false, "android", "metlifeapps.metlifeus", "lucinda201212@gmail.com", "lucindale8")]
		
		// local
		[TestCase("http://192.168.86.155:1234/qwe:admin:12232412", true)]
		[TestCase("http://10.10.20.1/:admin:MasiWifi*10", false, "http", "20.1", "admin", "MasiWifi*10")]
		[TestCase("http://10.10.20.1:10000/:admin:MasiWifi*10", false, "http", "20.1", "admin", "MasiWifi*10")]
		public void Test_CredentialsConvertion(string str, bool is_null, string protocol = "", string domain = "", string username = "", string password = "") {
			Credential? credentials = str;

			if (is_null) {
				Assert.That(credentials, Is.Null);
			}
			else {
				Assert.Multiple(
					() => {
						Assert.That(credentials.Protocol, Is.EqualTo(protocol));
						Assert.That(credentials.Domain, Is.EqualTo(domain));
						Assert.That(credentials.Username, Is.EqualTo(username));
						Assert.That(credentials.Password, Is.EqualTo(password));
					}
				);
			}
			
			TestContext.Out.WriteLine(JsonConvert.SerializeObject(credentials, Formatting.Indented));
		}

		[Test]
		public async Task Test_AddRangeFromFile() {
			await _credentials.AddRangeAsync(_configuration.DatabaseFilepath, new SqlServerBulkInsertOptions() {
				BatchSize = 1_000_000,
				BulkCopyTimeout = null,
				EnableStreaming = true
			});
			
			TestContext.Out.WriteLine(_context.Set<Credential>().Count());
		}
	}

	public class DatabaseConfiguration {
		public string DatabaseFilepath { get; set; }
	}
}