using Arkko.MomoTalk.Foundation.Utils;
using Arkko.MomoTalk.OneBot.Protocol.Events;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class ObFace : MessageBase {
    internal override string TypeId => "face";

    public int Id { get; set; } = int.MinValue;

    public JsonElement? Raw { get; set; } // wuts this

    public string? ResultId { get; set; }

    public int? ChainCount { get; set; }

    internal ObFace() { }

    public ObFace(int id) {
        Id = id;
    }

    public override string ToContentString() {
        return $"[表情 #{Id}]";
    }

    internal override ArrayMessage PackArrayMessage() {
        return new ArrayMessage(TypeId, new {
            Id,
        });
    }

    internal override MessageBase[] UnpackArrayMessageJson(EventMessage ev, in JsonElement j) {
        JsonElement data = j.GetProperty("data");

        return [
            new ObFace(Id) {
                Raw = data.GetPropertyOrNull("raw"),
                ResultId = data.GetPropertyOrNull("resultId")?.GetRawText(),
                ChainCount = data.GetPropertyOrNull("chainCount")?.GetInt32(),
            },
        ];
    }
}
