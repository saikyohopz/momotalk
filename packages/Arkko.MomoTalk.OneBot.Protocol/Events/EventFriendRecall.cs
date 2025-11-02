namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventFriendRecall : EventNotice {
    public long UserId { get; set; }
    public long MessageId { get; set; }
}
