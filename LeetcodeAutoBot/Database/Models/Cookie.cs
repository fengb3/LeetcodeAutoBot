using Microsoft.Playwright;
using Riok.Mapperly.Abstractions;

namespace LeetcodeAutoBot.Database.Models;

public class Cookie : Microsoft.Playwright.Cookie
{
    public int AccountId { get; set; }
}

[Mapper]
public static partial class CookieExtensions
{
    [MapperIgnoreTarget(nameof(Cookie.AccountId))]
    [MapperIgnoreTarget(nameof(Cookie.Url))]
    public static partial Cookie ToModel(this BrowserContextCookiesResult cookie);

    public static partial Cookie[] ToModels(this IEnumerable<BrowserContextCookiesResult> cookies);
	
    public static partial Microsoft.Playwright.Cookie FromModel(this Cookie cookie);
	
}
