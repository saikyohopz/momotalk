using Arkko.MomoTalk.Utils;
using System.Text.Json;

namespace Arkko.MomoTalk.OneBot;

public class ApiTask : TaskCompletionSource<JsonElement> {
    private readonly Type _returnType;

    private readonly bool _nullable;

    public ApiTask(Type returnType, bool nullable = false) {
        _returnType = returnType;
        _nullable = nullable;
    }

    public async Task<T> GetDeserializedResultAsync<T>() {
        Type actualType = typeof(T);

        if (actualType != _returnType) {
            throw new InvalidOperationException(
                $"type mismatch, expected: {_returnType.Name}, actual: {actualType.Name}"
            );
        }

        JsonElement o = await Task.ConfigureAwait(false);

        if (!_nullable && o.ValueKind == JsonValueKind.Null) {
            throw new NullReferenceException("not nullable value is null");
        }
        
        return o.Deserialize<T>(JsonOptions.SnakeCaseInstance)!;
    }
}
