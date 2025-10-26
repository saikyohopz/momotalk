using Arkko.MomoTalk.OneBot.Models;

namespace Arkko.MomoTalk.OneBot.Events;

public class EventGroupUpload : EventNotice {
    public long GroupId { get; set; }
    public long UserId { get; set; }
    public required ObFileInfo File { get; set; }
}
