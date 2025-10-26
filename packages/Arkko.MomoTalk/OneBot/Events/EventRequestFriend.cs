namespace Arkko.MomoTalk.OneBot.Events;

public class EventRequestFriend : EventRequest {
    public long UserId { get; set; }
    public required string Comment { get; set; }
    public required string Flag { get; set; }
}
