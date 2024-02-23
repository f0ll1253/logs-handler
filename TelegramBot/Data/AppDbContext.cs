using Microsoft.EntityFrameworkCore;

namespace TelegramBot.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User>? Users { get; set; }
    public DbSet<InviteCode>? InviteCodes { get; set; }
    public DbSet<File>? Files { get; set; }

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
        
        builder.Entity<File>(x =>
        {
            x.Property(x => x.Id).IsRequired();
            x.ToTable("Files");
        });

        base.OnModelCreating(builder);
    }
}