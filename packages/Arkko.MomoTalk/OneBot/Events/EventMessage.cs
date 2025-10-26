using Arkko.MomoTalk.OneBot.Messages;
using Arkko.MomoTalk.OneBot.Models;
using System.Text.Json.Serialization;

namespace Arkko.MomoTalk.OneBot.Events;

public class EventMessage : Event {
    public required string MessageType { get; set; }
    
    public required string SubType { get; set; }
    
    public int MessageId { get; set; }
    
    public long UserId { get; set; }

    [JsonIgnore] // too complex to deserialize, do it by ourselves
    public required MessageChain Message;

    public required string RawMessage { get; set; }
    
    public int Font { get; set; }
    
    public required ObMessageSender Sender { get; set; }
}
