using Bot.Models;

using Microsoft.EntityFrameworkCore;

namespace Bot.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options) {
    public DbSet<ApplicationUser> Users { get; set; }
}