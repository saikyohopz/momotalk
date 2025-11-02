using Arkko.MomoTalk.OneBot.Protocol.Events;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class ObReply : MessageBase {
    internal override string TypeId => "reply";

    public string Id { get; set; }

    public ObReply(string id) {
        Id = id;
    }

    public override string ToContentString() {
        return $"[引用消息 #{Id}]";
    }

    internal override ArrayMessage PackArrayMessage() {
        return new ArrayMessage(TypeId, new {
            Id,
        });
    }

    internal override MessageBase[] UnpackArrayMessageJson(EventMessage ev, in JsonElement j) {
        JsonElement data = j.GetProperty("data");

        return [new ObReply(data.GetProperty("id").GetString()!)];
    }
}
