using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Hosting.Conversions;

public interface IMessageChainParser {
    MessageChain Parse(object? o);
}

public interface IMessageChainParser<in T> : IMessageChainParser {
    MessageChain IMessageChainParser.Parse(object? o) {
        return Parse((T)o!);
    }

    MessageChain Parse(T o);
}
