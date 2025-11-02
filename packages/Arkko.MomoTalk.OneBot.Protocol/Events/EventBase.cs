namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public abstract class EventBase {
    public long Time { get; set; }
    public long SelfId { get; set; }
    public required string PostType { get; set; }
}
