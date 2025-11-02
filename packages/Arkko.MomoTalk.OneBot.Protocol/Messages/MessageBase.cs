using Arkko.MomoTalk.OneBot.Protocol.Events;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public abstract class MessageBase {
    internal abstract string TypeId { get; }
    
    public abstract string ToContentString();

    internal abstract ArrayMessage PackArrayMessage();

    internal abstract MessageBase[] UnpackArrayMessageJson(EventMessage ev, in JsonElement j);
}
