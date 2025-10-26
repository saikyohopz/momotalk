namespace Arkko.MomoTalk.OneBot.Events;

public class EventGroupRecall : EventNotice {
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public long OperatorId { get; set; }
    public long MessageId { get; set; }
}
