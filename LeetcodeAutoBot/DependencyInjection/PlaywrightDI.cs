using LeetcodeAutoBot.Database;
using LeetcodeAutoBot.Database.Models;
using LeetcodeAutoBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Cookie=LeetcodeAutoBot.Database.Models.Cookie;

namespace LeetcodeAutoBot.DependencyInjection;

public static class PlaywrightDI
{
    public static IServiceCollection AddPlaywright(this IServiceCollection services)
    {
        services.AddSingleton<IPlaywright>(_ =>
        {
            var playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
            return playwright;
        });

        services.AddScoped<IBrowserContext>(sp =>
        {
            var playwright     = sp.GetRequiredService<IPlaywright>();
            var accountSession = sp.GetRequiredService<AccountSession>();
            var isCi           = Environment.GetEnvironmentVariable("CI") == "true";
            
            // 使用持久化上下文,保持稳定的浏览器指纹
            var browserContext = playwright
                .Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = isCi,
                        Args     = ["--disable-blink-features=AutomationControlled"],
                    }
                )
                .GetAwaiter()
                .GetResult()
                .NewContextAsync(
                    new BrowserNewContextOptions
                    {
                       UserAgent =
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                            "AppleWebKit/537.36 (KHTML, like Gecko) " +
                            "Chrome/131.0.0.0 Safari/537.36",
                    }
                )
                .GetAwaiter()
                .GetResult()
                ;
            
            browserContext
                .AddInitScriptAsync(@"
                    Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                    Object.defineProperty(navigator, 'platform', { get: () => 'Win32' });
                ")
                .GetAwaiter().GetResult();
            
            // 注入 LocalStorage
            var localStorage = accountSession.LocalStorage;
            if (localStorage.Count > 0)
            {
                Console.WriteLine($"[PlaywrightDI] Injecting {localStorage.Count} localStorage items.");
                var json = System.Text.Json.JsonSerializer.Serialize(localStorage);
                browserContext.AddInitScriptAsync($$"""
                    (function() {
                        try {
                            const data = {{json}};
                            // 简单判断，避免污染其他域，虽然这里主要访问 leetcode
                            if (window.location.hostname.includes('leetcode')) {
                                for (const key in data) {
                                    window.localStorage.setItem(key, data[key]);
                                }
                            }
                        } catch (e) { console.error('Failed to restore localStorage', e); }
                    })();
                """).GetAwaiter().GetResult();
            }
            
            // 如果有保存的 cookies,尝试添加(持久化上下文会自动保存 cookies)
            var cookies = accountSession.AccountCookies;
            if (cookies.Length > 0)
            {
                try
                {
                    // 确保 Cookie 的 Domain 设置正确
                    // 有些 Cookie 可能只设置了 .leetcode.cn，但子域名访问时可能需要明确
                    // 这里我们不做过多修改，直接信任 Playwright 的处理
                    browserContext.AddCookiesAsync(cookies).GetAwaiter().GetResult();
                }
                catch
                {
                    // 如果添加失败,继续执行(可能是 cookies 已经存在)
                }
            }

            return browserContext;
        });

        services.AddTransient<IPage>(provider =>
        {
            var context = provider.GetRequiredService<IBrowserContext>();
            var page = context.NewPageAsync().GetAwaiter().GetResult();
            // page.Close += async (_, p) =>
            // {
            //     var cookies = await p.Context.CookiesAsync();
            //     provider.GetRequiredService<AccountSession>().AccountCookies = cookies.ToModels();
            // };
            return page;
        });

        return services;
    }

    public static IHost UsePlaywright(this IHost host)
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            throw new Exception($"Playwright browser installation failed with exit code {exitCode}");
        }
        return host;
    }
}

public static class DI
{
    public static IServiceCollection AddLeetCodeAutoBot(this IServiceCollection services)
    {
        services.AddPlaywright();
        services.AddScoped<ILeetcodeProblemSolver, LeetcodeProblemSolver>();
        services.AddScoped<AccountSession>();
        services.AddDbContext<LeetcodeAutoBotDbContext>(options =>
        {
            options.UseSqlite("Data Source=leetcodeautobot.db");
        });
        return services;
    }
    
    public static IHost UseLeetCodeAutoBot(this IHost host)
    {
        host.UsePlaywright();
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LeetcodeAutoBotDbContext>();
        dbContext.Database.Migrate();
        return host;
    }
}


