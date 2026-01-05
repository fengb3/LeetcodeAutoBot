using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LeetcodeAutoBot.Database;

public class SnapBuyDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LeetcodeAutoBotDbContext>
{
    public LeetcodeAutoBotDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LeetcodeAutoBotDbContext>();
        optionsBuilder.UseSqlite("", b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
        return new LeetcodeAutoBotDbContext(optionsBuilder.Options);
    }
}