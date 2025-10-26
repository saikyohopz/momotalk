using Arkko.MomoTalk.OneBot;

namespace Arkko.MomoTalk.Boot;

public static class MomoTalkBootstrap {
    public async static Task<IMomoTalk> Run(
        string webSocketUrl, Action<MomoTalkConfig>? configure = null
    ) {
        MomoTalkConfig conf = new(webSocketUrl);

        configure?.Invoke(conf);

        PositiveMomoTalkBot momoTalk = new(conf);

        await momoTalk.ConnectAsync();

        return momoTalk;
    }
}
