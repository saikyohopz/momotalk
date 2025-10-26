using Arkko.MomoTalk.Example;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging
    .SetMinimumLevel(LogLevel.Trace)
    .AddSimpleConsole(options => {
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    });

builder.Services.AddHostedService<MomoTalkHostedService>();

await builder.Build().RunAsync();
