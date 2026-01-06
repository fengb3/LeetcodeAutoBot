using LeetcodeAutoBot.Database;
using LeetcodeAutoBot.Database.Models;
using LeetcodeAutoBot.Helper;
using Microsoft.EntityFrameworkCore;

namespace LeetcodeAutoBot.Services;

public record AccountSession(IServiceProvider sp, IConfiguration config)
{
    public int AccountId { get; set; }

    public string Device => config["Device"] ?? (Environment.GetEnvironmentVariable("CI") == "true" ? "GitHubActions" : "Local");

    public Cookie[] AccountCookies
    {
        get
        {
            var dbContext = sp.GetRequiredService<LeetcodeAutoBotDbContext>();
            var cookies   = dbContext.Cookies.AsNoTracking().Where(c => c.AccountId == AccountId).ToArray();
            return cookies;
        }
        set
        {
            var dbContext     = sp.GetRequiredService<LeetcodeAutoBotDbContext>();
            var cookies = value.Select(c =>
            {
                c.AccountId = AccountId;
                return c;
            }).ToArray();
            dbContext.Cookies.AddOrUpdate(c => new { c.Name, c.Domain, c.AccountId }, cookies);
            dbContext.SaveChanges();
        }
    }

    public Dictionary<string, string> LocalStorage
    {
        get
        {
            var dbContext = sp.GetRequiredService<LeetcodeAutoBotDbContext>();
            var entries   = dbContext.LocalStorageEntries.Where(e => e.AccountId == AccountId).ToArray();

            return entries.ToDictionary(e => e.Key, e => e.Value);
        }
        set
        {
            var dbContext     = sp.GetRequiredService<LeetcodeAutoBotDbContext>();

            var entries = value.Select(kv => new LocalStorageEntry
            {
                AccountId = AccountId,
                Key       = kv.Key,
                Value     = kv.Value,
            }).ToArray();
            
            dbContext.LocalStorageEntries.AddOrUpdate(e => new { e.Key, e.AccountId }, entries);
            dbContext.SaveChanges();
        }
    }
}