namespace Arkko.MomoTalk.OneBot.Events;

public class EventGroupAdmin : EventNotice {
    public required string SubType { get; set; }
    public long GroupId { get; set; }
    public long UserId { get; set; }
}
