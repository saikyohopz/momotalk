using Arkko.MomoTalk.OneBot.Protocol.Messages;
using Arkko.MomoTalk.OneBot.Protocol.Models;
using System.Text.Json.Serialization;

namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventMessage : EventBase, IEventQuickOptAware {
    [JsonIgnore]
    public Func<object, Task>? QuickOptInvoker { get; set; }
    
    public required string MessageType { get; set; }
    
    public required string SubType { get; set; }
    
    public int MessageId { get; set; }
    
    public long UserId { get; set; }

    [JsonIgnore] // too complex to deserialize, do it by ourselves
    public required MessageChain Message;

    public required string RawMessage { get; set; }
    
    public int Font { get; set; }
    
    public required ObMessageSender Sender { get; set; }

    public async Task ReplyAsync(MessageChain messageChain) {
        if (this is EventMessageGroup emg) {
            await emg.ReplyAsync(messageChain);
        } else if (this is EventMessagePrivate emp) {
            await emp.ReplyAsync(messageChain);
        }
    }
}
