namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class MessageChain : List<MessageBase> {
    public static MessageChainBuilder Builder => new();

    public MessageChain() { }

    public MessageChain(IList<MessageBase> messages) : base(messages) { }

    public string ToContentString() {
        return string.Concat(from msg in this select msg.ToContentString());
    }
}
