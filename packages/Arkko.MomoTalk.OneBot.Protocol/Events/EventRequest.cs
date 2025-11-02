using Arkko.MomoTalk.OneBot.Protocol.Enums;

namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventRequest : EventBase {
    public RequestType RequestType { get; set; }
}
