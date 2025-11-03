namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventPoke : EventNoticeNotify {
    public long UserId { get; set; }
    public long TargetId { get; set; }
}
