using Arkko.MomoTalk;
using Arkko.MomoTalk.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging
    .SetMinimumLevel(LogLevel.Trace)
    .AddSimpleConsole(options => { options.TimestampFormat = "yyyy-MM-dd HH:mm:ss"; });

builder.UseMomoTalk(new MomoTalkConfig("ws://arkko.cc:32008") {
    Token = "saikyo",
});

await builder.Build().RunAsync();
