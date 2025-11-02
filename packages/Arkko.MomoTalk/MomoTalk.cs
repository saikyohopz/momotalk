using Arkko.MomoTalk.Boot;
using Arkko.MomoTalk.Common;
using Arkko.MomoTalk.Hosting;
using Arkko.MomoTalk.OneBot;
using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Arkko.MomoTalk.OneBot.Protocol.Models;

namespace Arkko.MomoTalk;

public class MomoTalk {
    internal OneBotClient OneBot { get; }

    public long BotId => throw new NotImplementedException();

    public MomoTalk(MomoTalkConfig config) {
        OneBot = new OneBotClient(
            config.WebSocketUrl, config.Token, config.RetryTimes, config.RetryWait,
            config.RetryRest, config.WebSocketHeartbeat, config.LoggerFactory, this
        );
    }

    public async Task<ObMessageId> SendPrivateMessage(long userId, MessageChain messageChain) {
        return await OneBot.Apis.SendPrivateMessage(userId, messageChain);
    }

    public async Task ConnectAsync() {
        await OneBot.ConnectAsync();
    }

    public async Task CloseAsync() {
        await OneBot.CloseAsync();
    }

    public bool IsConnected => OneBot.IsConnected;
}
