using System.Collections.Immutable;

namespace Arkko.MomoTalk.Utils;

public static class Dict {
    public static IDictionary<TK, TV> Of<TK, TV>(params (TK k, TV v)[] kvs) where TK : notnull {
        ImmutableDictionary<TK, TV>.Builder builder = ImmutableDictionary.CreateBuilder<TK, TV>();

        foreach ((TK k, TV v) in kvs) {
            builder.Add(k, v);
        }

        return builder.ToImmutable();
    }
}
