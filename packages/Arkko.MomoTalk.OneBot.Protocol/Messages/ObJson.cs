using Arkko.MomoTalk.OneBot.Protocol.Events;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class ObJson : MessageBase {
    internal ObJson() { }

    public ObJson(JsonElement data) {
        Data = data;
    }

    internal override string TypeId => "json";

    public JsonElement Data { get; set; }

    public override string ToContentString() {
        return $"[JSON {Data}]";
    }

    internal override ArrayMessage PackArrayMessage() {
        return new ArrayMessage(TypeId, new {
            Data,
        });
    }

    internal override MessageBase[] UnpackArrayMessageJson(EventMessage ev, in JsonElement j) {
        JsonElement data = j.GetProperty("data");

        return [
            new ObJson(data.GetProperty("data")),
        ];
    }
}
