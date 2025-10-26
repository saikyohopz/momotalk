using Arkko.MomoTalk.OneBot;

namespace Arkko.MomoTalk;

public interface IMomoTalk {
    bool IsConnected { get; }
    
    OneBotClient OneBot { get; }
    
    Task ConnectAsync();
    
    Task CloseAsync();
}
