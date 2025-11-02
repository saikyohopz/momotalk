namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventGroupAdmin : EventNotice {
    public required string SubType { get; set; }
    public long GroupId { get; set; }
    public long UserId { get; set; }
}
