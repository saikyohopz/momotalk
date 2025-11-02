using System.Text;

namespace Arkko.MomoTalk.Foundation.Utils;

internal static class CqUtils {
    public static string BuildCqCodeString(string type, IDictionary<string, object> data) {
        StringBuilder sb = new StringBuilder().Append("[CQ:").Append(type);

        foreach ((string k, object v) in data) {
            object value = v;
            
            if (v is bool) {
                value = Convert.ToInt32(v);
            }
            
            string stringified = EscapeSpecialChars(value.ToString() ?? string.Empty);

            sb.Append(k).Append('=').Append(stringified);
        }

        sb.Append(']');

        return sb.ToString();
    }

    public static string EscapeSpecialChars(string input) {
        if (string.IsNullOrEmpty(input)) {
            return string.Empty;
        }

        StringBuilder sb = new(input.Length * 2);

        foreach (char c in input) {
            switch (c) {
            case '&':
                sb.Append("&amp;");
                break;
            case '[':
                sb.Append("&#91;");
                break;
            case ']':
                sb.Append("&#93;");
                break;
            case ',':
                sb.Append("&#44;");
                break;
            default:
                sb.Append(c);
                break;
            }
        }

        return sb.ToString();
    }

    public static string ToSnakeCaseString(this string camelCase) {
        if (string.IsNullOrEmpty(camelCase)) {
            return camelCase;
        }

        StringBuilder snakeCaseBuilder = new();
        snakeCaseBuilder.Append(char.ToLower(camelCase[0]));

        for (int i = 1; i < camelCase.Length; i++) {
            char currentChar = camelCase[i];

            if (char.IsUpper(currentChar)) {
                snakeCaseBuilder.Append('_');
                snakeCaseBuilder.Append(char.ToLower(currentChar));
            } else {
                snakeCaseBuilder.Append(currentChar);
            }
        }

        return snakeCaseBuilder.ToString();
    }
}
