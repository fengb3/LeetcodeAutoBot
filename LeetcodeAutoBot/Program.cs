using LeetcodeAutoBot;
using LeetcodeAutoBot.DependencyInjection;
using LeetcodeAutoBot.Services;
using Serilog;

const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u4}] {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.File("logs/log.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
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