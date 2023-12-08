using Microsoft.EntityFrameworkCore;
using TL;

namespace TelegramBot.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<InviteCode> InviteCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(x =>
        {
            x.Property(x => x.id).IsRequired();
            x.ToTable("Users");
        });

        builder.Entity<InviteCode>(x =>
        {
            x.Property(x => x.Id).IsRequired();
            x.ToTable("InviteCodes");
        });
        
        base.OnModelCreating(builder);
    }
}