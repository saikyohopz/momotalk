namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventGroupBan : EventNotice {
    public required string SubType { get; set; }
    public long GroupId { get; set; }
    public long OperatorId { get; set; }
    public long UserId { get; set; }
    public long Duration { get; set; }
}
