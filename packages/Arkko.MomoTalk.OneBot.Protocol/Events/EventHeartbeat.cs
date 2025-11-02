using Arkko.MomoTalk.OneBot.Protocol.Models;

namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventHeartbeat : EventMeta {
    public required ObConnectionStatus Status { get; set; }
    public long Interval { get; set; }
}
