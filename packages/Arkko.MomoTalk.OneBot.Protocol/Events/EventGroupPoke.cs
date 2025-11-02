namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventGroupPoke : EventNoticeNotify {
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public long TargetId { get; set; }
}
