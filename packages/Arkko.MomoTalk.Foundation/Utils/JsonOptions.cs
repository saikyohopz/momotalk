using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arkko.MomoTalk.Foundation.Utils;

public static class JsonOptions {
    public static JsonSerializerOptions DefaultInstance { get; } = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static JsonSerializerOptions SnakeCaseInstance { get; } = new() {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(new SnakeCaseJsonNamingPolicy()) },
    };
}
