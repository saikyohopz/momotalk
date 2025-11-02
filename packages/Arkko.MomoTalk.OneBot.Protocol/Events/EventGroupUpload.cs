using Arkko.MomoTalk.OneBot.Protocol.Models;

namespace Arkko.MomoTalk.OneBot.Protocol.Events;

public class EventGroupUpload : EventNotice {
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public required ObFileInfo File { get; set; }
}
