namespace Arkko.MomoTalk.OneBot.Protocol.Messages;

public class MessageChain : List<MessageBase> {
    public MessageChain() { }

    public MessageChain(IList<MessageBase> messages) : base(messages) { }
    public static MessageChainBuilder Builder => new();

    public string ToContentString() {
        return string.Concat(from msg in this select msg.ToContentString());
    }

    public static MessageChain BuildTextMessage(string text) {
        return Builder.Text(text).Build();
    }
}
