namespace Arkko.MomoTalk.Foundation.Utils;

public static class ObjectUtils {
    public static bool TryGet<T>(this T? o, out T? v) {
        v = o;

        return v != null;
    }

    public static Task<bool> TryGetAsync<T>(this Task<T?> o, out T? v) {
        v = o.ConfigureAwait(false).GetAwaiter().GetResult();

        return Task.FromResult(v != null);
    }
}
