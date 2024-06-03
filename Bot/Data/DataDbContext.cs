using Bot.Models.Data;

using Microsoft.EntityFrameworkCore;

namespace Bot.Data;

public class DataDbContext(DbContextOptions<DataDbContext> options) : DbContext(options) {
    public DbSet<FileEntity> Files { get; set; }
    public DbSet<FileTelegramInfo> TelegramInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<FileEntity>(entity =>
        {
            entity.HasOne(x => x.TelegramInfo)
                .WithOne(x => x.File)
                .HasPrincipalKey<FileEntity>(x => x.Id)
                .HasForeignKey<FileTelegramInfo>(x => x.FileId);
        });

        builder.Entity<FileTelegramInfo>(entity =>
        {
            entity.HasOne(x => x.File)
                .WithOne(x => x.TelegramInfo)
                .HasPrincipalKey<FileTelegramInfo>(x => x.Id)
                .HasForeignKey<FileEntity>(x => x.TelegramInfoId);
        });
        
        base.OnModelCreating(builder);
    }
}