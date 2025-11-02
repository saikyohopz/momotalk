using Arkko.MomoTalk.Boot;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Arkko.MomoTalk.Hosting.Services;

[Singleton]
public class BotCollectionService {
    private readonly ConcurrentDictionary<long, MomoTalk> _bots = [];
    
    private readonly ILoggerFactory _loggerFactory;
    
    private readonly IServiceProvider _serviceProvider;

    public BotCollectionService(ILoggerFactory loggerFactory, IServiceProvider serviceProvider) {
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
    }

    public void CreateBot(params MomoTalkConfig[] configs) {
        Task[] tasks = new Task[configs.Length];

        for (int i = 0; i < configs.Length; i++) {
            MomoTalk momoTalk = new(configs[i]);

            momoTalk.OneBot.EventHandlers.RegisterEventHandlerByAttribute(_serviceProvider);

            tasks[i] = Task.Run(async () => {
                await momoTalk.ConnectAsync();
                _bots[momoTalk.BotId] = momoTalk;
            });
        }
        
        Task.WaitAll(tasks);
    }

    public MomoTalk GetBot(long botId) {
        return _bots[botId];
    }

    public void CloseAll() {
        Task[] tasks = new Task[_bots.Count];

        for (int i = 0; i < _bots.Count; i++) {
            MomoTalk bot = _bots[i];
            
            tasks[i] = Task.Run(async () => {
                await bot.CloseAsync();
                _bots.Remove(bot.BotId, out _);
            });
        }
        
        Task.WaitAll(tasks);
    }

    public ICollection<MomoTalk> GetAllBots() {
        return _bots.Values;
    }
}
