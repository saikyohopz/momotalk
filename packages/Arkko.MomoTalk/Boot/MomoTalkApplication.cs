using Arkko.MomoTalk.Hosting;
using Arkko.MomoTalk.Hosting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arkko.MomoTalk.Boot;

public static class MomoTalkApplication {
    public async static Task Run(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Logging
            .SetMinimumLevel(LogLevel.Trace)
            .AddSimpleConsole(options => {
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
            });

        builder.Services
            .AddScopedByAttribute()
            .AddSingletonByAttribute()
            .AddHostedService<MomoTalkHostedService>();

        await builder.Build().RunAsync();
    }
}
