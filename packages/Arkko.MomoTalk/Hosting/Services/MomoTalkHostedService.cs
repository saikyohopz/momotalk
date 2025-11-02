using Arkko.MomoTalk.Boot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arkko.MomoTalk.Hosting.Services;

public class MomoTalkHostedService(
    ILoggerFactory loggerFactory,
    BotCollectionService botCollectionService
) : IHostedService {
    public Task StartAsync(CancellationToken cancellationToken) {
        botCollectionService.CreateBot(new MomoTalkConfig("ws://arkko.cc:32008") {
            Token = "saikyo",
            LoggerFactory = loggerFactory,
        });
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        botCollectionService.CloseAll();
        
        return Task.CompletedTask;
    }
}
