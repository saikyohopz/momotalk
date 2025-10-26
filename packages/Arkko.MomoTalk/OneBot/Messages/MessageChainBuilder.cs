namespace Arkko.MomoTalk.OneBot.Messages;

public class MessageChainBuilder {
    private readonly List<MessageBase> _messages = [];

    public MessageChainBuilder Append(MessageBase messageBase) {
        _messages.Add(messageBase);

        return this;
    }

    public MessageChain Build() {
        return new MessageChain(_messages);
    }
}
