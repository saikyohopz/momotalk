using System.Collections.Immutable;

namespace Arkko.MomoTalk.Foundation.Utils;

public static class DictUtils {
    public static IDictionary<TK, TV> MakeImmutableMap<TK, TV>(params (TK k, TV v)[] kvs) where TK : notnull {
        ImmutableDictionary<TK, TV>.Builder builder = ImmutableDictionary.CreateBuilder<TK, TV>();

        foreach ((TK k, TV v) in kvs) {
            builder.Add(k, v);
        }

        return builder.ToImmutable();
    }

    public static TV ComputeIfAbsent<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TK, TV> provider) {
        if (!dict.TryGetValue(key, out TV? value)) {
            value = provider.Invoke(key);
            
            dict.Add(key, value);
        }
        
        return value;
    }
}
