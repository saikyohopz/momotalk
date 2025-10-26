namespace Arkko.MomoTalk.OneBot.Events;

public class EventFriendRecall : EventNotice {
    public long UserId { get; set; }
    public long MessageId { get; set; }
}
