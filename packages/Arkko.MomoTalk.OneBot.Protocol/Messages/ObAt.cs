using Arkko.MomoTalk.OneBot.Protocol.Events;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class ObAt : MessageBase {
    internal override string TypeId => "at";

    public long? GroupId { get; set; }

    public long UserId { get; set; } = -1;

    internal ObAt() { }

    public ObAt(long userId) {
        UserId = userId;
    }

    public override string ToContentString() {
        return UserId == -1 ? "@全体成员" : $"@{UserId}";
    }

    internal override ArrayMessage PackArrayMessage() {
        return new ArrayMessage(TypeId, new {
            Qq = UserId == -1 ? "all" : UserId.ToString(),
        });
    }

    internal override MessageBase[] UnpackArrayMessageJson(EventMessage ev, in JsonElement j) {
        JsonElement data = j.GetProperty("data");
        string userId = data.GetProperty("qq").GetString() ?? "all";
        long? groupId = ev is EventMessageGroup emg ? emg.GroupId : null;

        if (userId == "all") {
            return [
                new ObAt(-1) {
                    GroupId = groupId,
                },
            ];
        }

        return [
            new ObAt(long.Parse(userId)) {
                GroupId = groupId,
            },
        ];
    }
}
