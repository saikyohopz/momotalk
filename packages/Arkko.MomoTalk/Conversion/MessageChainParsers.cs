using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Conversion;

public static class MessageChainParsers {
    public class FromSelf : IMessageChainParser<MessageChain> {
        public MessageChain Parse(MessageChain o) {
            return o;
        }
    }

    public class ObjectToText : IMessageChainParser<object> {
        public MessageChain Parse(object o) {
            return MessageChain.Builder.Text(Convert.ToString(o) ?? string.Empty).Build();
        }
    }

    public class FromMessage : IMessageChainParser<MessageBase> {
        public MessageChain Parse(MessageBase o) {
            return MessageChain.Builder.Append(o).Build();
        }
    }
}
