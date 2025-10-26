using Arkko.MomoTalk.Boot;
using Arkko.MomoTalk.OneBot.Events;
using Arkko.MomoTalk.OneBot.Messages;
using Arkko.MomoTalk.OneBot.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Arkko.MomoTalk.Example;

public class MomoTalkHostedService(ILoggerFactory loggerFactory, ILogger<MomoTalkHostedService> logger) : IHostedService {
    private IMomoTalk _momoTalk = new NullMomoTalkBot();

    public async Task StartAsync(CancellationToken cancellationToken) {
        _momoTalk = await MomoTalkBootstrap.Run("ws://arkko.cc:32008", conf => {
            conf.Token = "saikyo";
            conf.LoggerFactory = loggerFactory;
        });

        _momoTalk.OneBot.PrivateMessageEvent += OnPrivateMessageEvent;
        _momoTalk.OneBot.GroupMessageEvent += OnGroupMessageEvent;
    }

    private async static void OnPrivateMessageEvent(IMomoTalk momoTalk, EventMessagePrivate ev) {
        if (ev.UserId == 1781176460) {
            await momoTalk.OneBot.Apis.SendPrivateMessage(1781176460, ev.Message);
        }
    }

    private void OnGroupMessageEvent(IMomoTalk momoTalk, EventMessageGroup ev) {
        _ = 1;
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        await _momoTalk.CloseAsync();
    }
}
