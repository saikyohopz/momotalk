using System.Text.Json;

namespace Arkko.MomoTalk.Foundation.Utils;

public static class JsonExtensions {
    extension(object o) {
        public string SerializeJson(JsonSerializerOptions? options = null) {
            options ??= JsonOptions.DefaultInstance;

            return JsonSerializer.Serialize(o, options);
        }
    }

    extension(string s) {
        public T DeserializeFromJson<T>(JsonSerializerOptions? options = null) {
            return s.DeserializeFromJsonNullable<T>(options)
                ?? throw new NullReferenceException("not nullable value is null");
        }
        
        public T? DeserializeFromJsonNullable<T>(JsonSerializerOptions? options = null) {
            options ??= JsonOptions.DefaultInstance;
            
            return JsonSerializer.Deserialize<T>(s, JsonOptions.DefaultInstance);
        }
        
        public Dictionary<string, object?> DeserializeFromJson(JsonSerializerOptions? options = null) {
            return s.DeserializeFromJson<Dictionary<string, object?>>(options);
        }
        
        public Dictionary<string, object?>? DeserializeFromJsonNullable(JsonSerializerOptions? options = null) {
            return s.DeserializeFromJsonNullable<Dictionary<string, object?>>(options);
        }
    }

    extension(JsonElement e) {
        public JsonElement? GetPropertyOrNull(string name) {
            if (e.TryGetProperty(name, out JsonElement element)) {
                return element;
            }

            return null;
        }
    }
}
