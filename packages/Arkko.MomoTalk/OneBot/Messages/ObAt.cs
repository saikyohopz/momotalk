using Arkko.MomoTalk.Utils;

namespace Arkko.MomoTalk.OneBot.Messages;

public class ObAt : MessageBase {
    internal override string TypeIm => "at";

    public long UserId { get; set; }

    public ObAt(long userId) {
        UserId = userId;
    }

    public override string ToContentString() {
        return $"@{UserId}"; // todo user name
    }

    public override string ToCqCodeString() {
        return CqUtils.BuildCqCodeString(TypeIm, Dict.Of<string, object>(
            ("qq", UserId)
        ));
    }

    public override object ToArrayMessageData() {
        return new {
            Qq = UserId,
        };
    }
}
