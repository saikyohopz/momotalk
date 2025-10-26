using Arkko.MomoTalk.OneBot.Models;

namespace Arkko.MomoTalk.OneBot.Events;

public class EventMessageGroup : EventMessage {
    public long GroupId { get; set; }
    public ObAnonymous? Anonymous { get; set; }
}
