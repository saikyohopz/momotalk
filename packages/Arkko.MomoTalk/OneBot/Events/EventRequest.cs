using Arkko.MomoTalk.OneBot.Enums;

namespace Arkko.MomoTalk.OneBot.Events;

public class EventRequest : Event {
    public RequestType RequestType { get; set; }
}
