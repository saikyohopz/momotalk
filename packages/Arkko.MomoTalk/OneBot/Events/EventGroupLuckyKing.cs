namespace Arkko.MomoTalk.OneBot.Events;

public class EventGroupLuckyKing : EventNoticeNotify {
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public long TargetId { get; set; }
}
