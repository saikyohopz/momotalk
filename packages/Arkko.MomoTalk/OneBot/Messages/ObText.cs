using Arkko.MomoTalk.Utils;
using System.Net;

namespace Arkko.MomoTalk.OneBot.Messages;

public class ObText : MessageBase {
    internal override string TypeIm => "text";

    public string Text { get; set; }

    public ObText(string text) {
        Text = text;
    }

    public override string ToContentString() {
        return Text;
    }

    public override string ToCqCodeString() {
        return CqUtils.EscapeSpecialChars(Text);
    }

    public override object ToArrayMessageData() {
        return new {
            Text,
        };
    }
}
