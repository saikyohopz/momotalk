using Arkko.MomoTalk.OneBot.Models;

namespace Arkko.MomoTalk.OneBot.Events;

public class EventHeartbeat : EventMeta {
    public required ObConnectionStatus Status { get; set; }
    public long Interval { get; set; }
}
