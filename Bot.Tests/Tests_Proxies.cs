using Bot.Database;
using Bot.Models.Proxies;
using Bot.Services;

using Microsoft.EntityFrameworkCore;

namespace Bot.Tests;

public class Tests_Proxies {
	private DataDbContext _context;
	
	[OneTimeSetUp]
	public void OneTimeSetUp() {
		var builder = new DbContextOptionsBuilder<DataDbContext>();

		builder.UseInMemoryDatabase("Data");

		_context = new(builder.Options);

		_context.Database.EnsureCreated();

		_context.AddRange([
			new Proxy {
				Host = "host.1",
				Port = 1000,
				Username = "Username 1",
				Password = "Password 1"
			},
			new Proxy {
				Host = "host.2",
				Port = 2000,
				Username = "Username 2",
				Password = "Password 2"
			}
		]);

		_context.SaveChanges();
	}

	[Test, Order(0)]
	public void Test_CheckIndexesOrder() {
		var set = _context.Set<Proxy>()
						  .OrderByDescending(x => x.Index)
						  .ToList();
		
		for (int i = 1; i < set.Count; i++) {
			Assert.That(set[i].Index, Is.EqualTo(i));
		}
	}

	[Test, Order(1)]
	public async Task Test_GetAsync() {
		//
		var repository = new ProxiesRepository(_context);

		//
		var proxy = await repository.GetAsync();
		Assert.That(proxy.IsInUse, Is.True);

		proxy = await _context.FindAsync<Proxy>(proxy.Id);
		Assert.That(proxy.Index, Is.EqualTo(3));
	}

	[Test, Order(2)]
	public async Task Test_CloseAsync() {
		//
		var repository = new ProxiesRepository(_context);
		
		//
		var proxy = await _context.Set<Proxy>().FirstOrDefaultAsync(x => x.IsInUse);

		if (proxy is null) {
			throw new Exception("Proxy in use not found");
		}
		
		await repository.CloseAsync(proxy.Id);
		
		proxy = await _context.FindAsync<Proxy>(proxy.Id);
		
		Assert.Multiple(() =>
		{
			Assert.That(proxy.Index, Is.EqualTo(4));
			Assert.That(proxy.IsInUse, Is.False);
		});
	}
	
	//[Test, Order(1)]
	public async Task Test_GenerateOnAdd()
    {
        //
        var repository = new ProxiesRepository(_context);
		
		// Get
		var proxy = await repository.GetAsync();
		Assert.That(proxy.IsInUse, Is.True);

		proxy = await _context.FindAsync<Proxy>(proxy.Id);
		Assert.That(proxy.Index, Is.EqualTo(1));

		// Close
		await repository.CloseAsync(proxy.Id);
		
		proxy = await _context.FindAsync<Proxy>(proxy.Id);
		
        Assert.Multiple(() =>
        {
            Assert.That(proxy.Index, Is.EqualTo(1));
            Assert.That(proxy.IsInUse, Is.False);
        });
    }
}