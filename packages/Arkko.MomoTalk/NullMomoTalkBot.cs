using Arkko.MomoTalk.OneBot;

namespace Arkko.MomoTalk;

public class NullMomoTalkBot : IMomoTalk {
    public bool IsConnected => false;
    
    public OneBotClient OneBot => throw new NotImplementedException();

    public Task ConnectAsync() {
        throw new NotImplementedException();
    }

    public Task CloseAsync() {
        throw new NotImplementedException();
    }
}
