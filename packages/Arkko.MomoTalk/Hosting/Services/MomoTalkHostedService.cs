using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arkko.MomoTalk.Hosting.Services;

public class MomoTalkHostedService(
    ILoggerFactory loggerFactory,
    BotCollectionService botCollectionService
) : IHostedService {
    public Task StartAsync(CancellationToken cancellationToken) {
        botCollectionService.ConnectAll();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        botCollectionService.CloseAll();
        
        return Task.CompletedTask;
    }
}
