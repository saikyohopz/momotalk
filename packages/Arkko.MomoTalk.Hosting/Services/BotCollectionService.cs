using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Arkko.MomoTalk.Hosting.Services;

public class BotCollectionService {
    private readonly ConcurrentDictionary<long, MomoTalk> _connectedBots = [];

    private readonly ILoggerFactory _loggerFactory;

    private readonly ConcurrentBag<MomoTalk> _registeredBots = [];

    private readonly IServiceProvider _serviceProvider;

    public BotCollectionService(ILoggerFactory loggerFactory, IServiceProvider serviceProvider) {
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
    }

    public void ConnectAll() {
        List<Task> tasks = new(_registeredBots.Count);

        tasks.AddRange(_registeredBots.Select(momoTalk => Task.Run(async () => {
            if (!momoTalk.IsConnected) {
                await momoTalk.ConnectAsync();
                _connectedBots[momoTalk.BotId] = momoTalk;
            }
        })));

        Task.WaitAll(tasks);
    }

    public void CreateBot(params IEnumerable<MomoTalkConfig> configs) {
        foreach (MomoTalkConfig config in configs) {
            MomoTalk momoTalk = new(config, _loggerFactory);

            momoTalk.OneBot.EventHandlers.RegisterEventHandlerByAttribute(_serviceProvider);

            _registeredBots.Add(momoTalk);
        }
    }

    public MomoTalk GetBot(long botId) {
        return _connectedBots[botId];
    }

    public void CloseAll() {
        Task[] tasks = new Task[_connectedBots.Count];

        for (int i = 0; i < _connectedBots.Count; i++) {
            MomoTalk bot = _connectedBots[i];

            tasks[i] = Task.Run(async () => {
                await bot.CloseAsync();
                _connectedBots.Remove(bot.BotId, out _);
            });
        }

        Task.WaitAll(tasks);
    }

    public ICollection<MomoTalk> GetAllBots() {
        return _connectedBots.Values;
    }
}
