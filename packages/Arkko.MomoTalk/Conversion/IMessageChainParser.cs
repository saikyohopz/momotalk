using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Conversion;

public interface IMessageChainParser;

public interface IMessageChainParser<in T> : IMessageChainParser {
    MessageChain Parse(T o);
}
