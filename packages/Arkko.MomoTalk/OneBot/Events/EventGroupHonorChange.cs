namespace Arkko.MomoTalk.OneBot.Events;

public class EventGroupHonorChange : EventNoticeNotify {
    public long GroupId { get; set; }
    public required string HonorType { get; set; }
    public long UserId { get; set; }
}
