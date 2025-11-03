namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class MessageChainBuilder {
    private readonly List<MessageBase> _messages = [];

    public MessageChainBuilder AppendAll(params IEnumerable<MessageBase> messages) {
        _messages.AddRange(messages);

        return this;
    }

    public MessageChainBuilder Append(MessageBase messageBase) {
        _messages.Add(messageBase);

        return this;
    }

    public MessageChainBuilder Text(string text) {
        return Append(new ObText(text));
    }

    public MessageChainBuilder LineBreak() {
        return Text("\n");
    }

    public MessageChainBuilder Image(byte[] image) {
        return Append(new ObImage(image));
    }

    public MessageChainBuilder At(long userId) {
        return Append(new ObAt(userId));
    }

    public MessageChain Build() {
        return new MessageChain(_messages);
    }
}
