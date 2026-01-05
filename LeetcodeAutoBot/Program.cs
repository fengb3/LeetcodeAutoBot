using LeetcodeAutoBot;
using LeetcodeAutoBot.DependencyInjection;
using LeetcodeAutoBot.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();

builder.Services.AddLeetCodeAutoBot();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOptions<LeetcodeLoginOption>()
    .Bind(builder.Configuration.GetSection("LEETCODE"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.UseLeetCodeAutoBot();

host.Run();