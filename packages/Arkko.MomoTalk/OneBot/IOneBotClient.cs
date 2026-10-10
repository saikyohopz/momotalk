using Arkko.MomoTalk.OneBot.Protocol.Models;

namespace Arkko.MomoTalk.OneBot;

public interface IOneBotClient {
    bool IsConnected { get; }
    OneBotApi Apis { get; }
    OneBotEventHandlerRepository EventHandlers { get; }
    Task ConnectAsync();
    Task CloseAsync();
    Task<TResponse> SendApiRequestAsync<TResponse>(ApiRequest request, bool nullable = false);
}
