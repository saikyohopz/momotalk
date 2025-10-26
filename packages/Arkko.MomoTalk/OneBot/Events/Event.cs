namespace Arkko.MomoTalk.OneBot.Events;

public class Event {
    public long Time { get; set; }
    public long SelfId { get; set; }
    public required string PostType { get; set; }
}
