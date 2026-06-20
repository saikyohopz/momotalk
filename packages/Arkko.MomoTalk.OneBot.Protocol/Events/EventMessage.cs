using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Arkko.MomoTalk.OneBot.Protocol.Models;
using System.Text.Json.Serialization;

namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventMessage : EventBase, IEventQuickOptAware {
    [JsonIgnore] // too complex to deserialize, do it by ourselves
    public required MessageChain Message;

    public required string MessageType { get; set; }

    public required string SubType { get; set; }

    public int MessageId { get; set; }

    public long UserId { get; set; }

    public required string RawMessage { get; set; }

    public int Font { get; set; }

    public required ObMessageSender Sender { get; set; }

    [JsonIgnore]
    public Func<object, Task>? QuickOptInvoker { get; set; }

    /// <summary>
    /// 回复消息
    /// </summary>
    /// <param name="messageChain">消息链</param>
    /// <param name="atSender">是否@发送该消息的成员，私聊消息无效</param>
    public async Task ReplyAsync(MessageChain messageChain, bool atSender = true) {
        if (this is EventMessageGroup emg) {
            await emg.ReplyAsync(messageChain, atSender);
        } else if (this is EventMessagePrivate emp) {
            await emp.ReplyAsync(messageChain);
        }
    }
}
