using Microsoft.EntityFrameworkCore;

namespace LeetcodeAutoBot.Database;

public class LeetcodeAutoBotDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Models.Cookie> Cookies { get; set; }
    public DbSet<Models.LocalStorageEntry> LocalStorageEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Cookie>(builder =>
        {
            builder.HasKey(c => new { c.Name, c.Domain, c.AccountId});
        });

        modelBuilder.Entity<Models.LocalStorageEntry>(builder =>
        {
            builder.HasKey(e => new { e.Key, e.AccountId});
        });
    }
}