using LeetcodeAutoBot.Services;

namespace LeetcodeAutoBot.Helper;

public static class DependencyInjectionHelper
{
    public static IServiceScope CreateAccountScope(this IServiceProvider sp, int accountId)
    {
        // var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        var scope = sp.CreateScope();
        var accountSession = scope.ServiceProvider.GetRequiredService<AccountSession>();
        accountSession.AccountId = accountId;
        return scope;
    }
}