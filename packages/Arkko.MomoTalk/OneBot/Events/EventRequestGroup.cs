namespace Arkko.MomoTalk.OneBot.Events;

public class EventRequestGroup : EventRequest {
    public required string SubType { get; set; }
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public required string Comment { get; set; }
    public required string Flag { get; set; }
}
