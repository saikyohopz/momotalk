namespace Arkko.MomoTalk.Foundation.Utils;

public static class ObjectUtils {
    public static bool TryGet<T>(this T? o, out T? v) {
        v = o;

        return o != null;
    }
}
