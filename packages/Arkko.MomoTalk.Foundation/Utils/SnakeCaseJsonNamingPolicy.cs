using System.Text;
using System.Text.Json;

namespace Arkko.MomoTalk.Foundation.Utils;

public class SnakeCaseJsonNamingPolicy : JsonNamingPolicy {
    public override string ConvertName(string name) {
        if (string.IsNullOrEmpty(name))
            return name;

        StringBuilder result = new();
        
        result.Append(char.ToLowerInvariant(name[0]));

        for (int i = 1; i < name.Length; i++) {
            char c = name[i];

            if (char.IsUpper(c)) {
                result.Append('_');
                result.Append(char.ToLowerInvariant(c));
            } else {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}
