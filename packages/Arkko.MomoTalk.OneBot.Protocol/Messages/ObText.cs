using Arkko.MomoTalk.OneBot.Protocol.Events;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class ObText : MessageBase {
    internal override string TypeId => "text";

    public string Text { get; set; } = string.Empty;

    internal ObText() { }

    public ObText(string text) {
        Text = text;
    }

    public override string ToContentString() {
        return Text;
    }

    internal override ArrayMessage PackArrayMessage() {
        return new ArrayMessage(TypeId, new {
            Text,
        });
    }

    internal override MessageBase[] UnpackArrayMessageJson(EventMessage ev, in JsonElement j) {
        JsonElement data = j.GetProperty("data");

        IEnumerable<string> strings = SplitAndKeepDelimiters(data.GetProperty("text").GetString() ?? string.Empty, ' ');

        return strings.Select(MessageBase (str) => new ObText(str)).ToArray();
    }

    private static List<string> SplitAndKeepDelimiters(string s, char delim) {
        List<string> result = [];
        if (string.IsNullOrEmpty(s))
            return result;

        int currentIndex = 0;
        bool isInDelimiter = s[0] == delim;

        for (int i = 1; i < s.Length; i++) {
            bool currentIsDelimiter = s[i] == delim;

            if (currentIsDelimiter != isInDelimiter) {
                result.Add(s.Substring(currentIndex, i - currentIndex));
                currentIndex = i;
                isInDelimiter = currentIsDelimiter;
            }
        }

        // 添加最后一个片段
        result.Add(s[currentIndex..]);

        return result;
    }
}
