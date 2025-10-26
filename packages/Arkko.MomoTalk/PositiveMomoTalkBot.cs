using Arkko.MomoTalk.Boot;
using Arkko.MomoTalk.OneBot;

namespace Arkko.MomoTalk;

public class PositiveMomoTalkBot : IMomoTalk {
    public OneBotClient OneBot { get; }
    
    public PositiveMomoTalkBot(MomoTalkConfig config) {
        OneBot = new OneBotClient(
            config.WebSocketUrl, config.Token, config.RetryTimes, config.RetryWait,
            config.RetryRest, config.WebSocketHeartbeat, config.LoggerFactory, this
        );
    }

    public async Task ConnectAsync() {
        await OneBot.ConnectAsync();
    }

    public async Task CloseAsync() {
        await OneBot.CloseAsync();
    }

    public bool IsConnected => OneBot.IsConnected;
}
