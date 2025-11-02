using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventMessagePrivate : EventMessage {
    /// <summary>
    /// 回复消息
    /// </summary>
    /// <param name="messageChain">消息链</param>
    public async Task ReplyAsync(MessageChain messageChain) {
        if (QuickOptInvoker != null) {
            await QuickOptInvoker.Invoke(new {
                Reply = MessagePacker.PackArrayMessages(messageChain),
            });
        }
    }
}
