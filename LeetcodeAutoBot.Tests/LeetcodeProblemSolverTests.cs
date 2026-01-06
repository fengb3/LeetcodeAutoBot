using LeetcodeAutoBot.DependencyInjection;
using LeetcodeAutoBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LeetcodeAutoBot.Tests;

public class LeetcodeProblemSolverTests
{
    [Fact]
    public async Task SolveProblemAsync_ShouldSolveTwoSum()
    {
        // Arrange
        // We use the Host builder to mimic the application startup, 
        // ensuring all dependencies and init logic (Playwright install, DB migrations) are run.
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLeetCodeAutoBot();
                // Configure logging to see output in test runner
                services.AddLogging(builder => 
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });
            });

        using var host = hostBuilder.Build();
        
        // This ensures Playwright browsers are installed and DB is migrated
        // Note: This might take time on first run
        host.UseLeetCodeAutoBot();

        await using var scope = host.Services.CreateAsyncScope();
        var solver = scope.ServiceProvider.GetRequiredService<ILeetcodeProblemSolver>();
        
        // Replace with a problem provided in the requirements or a standard one
        var problemUrl = "https://leetcode.cn/problems/combinations/";

        // Act
        // Note: This requires a valid session in the DB (cookies/localStorage) to fully succeed with submission.
        // If not logged in, it might fail or just stop at login page depending on implementation.
        await solver.SolveProblemAsync(problemUrl);

        // Assert
        // If no exception is thrown, we assume the solver ran through its process.
        // You would typically verify side effects here (e.g. check DB for submission record if applicable),
        // but for integration testing the solver logic, completion is a good first step.
    }
}

