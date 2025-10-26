using System.Collections.ObjectModel;

namespace Arkko.MomoTalk.OneBot.Messages;

public class MessageChain : Collection<MessageBase> {
    public static MessageChainBuilder Builder => new();

    public MessageChain() { }

    public MessageChain(IList<MessageBase> messages) : base(messages) { }

    public string ToCqCodeString() {
        return string.Concat(from msg in this select msg.ToCqCodeString());
    }

    public List<ArrayMessage> ToArrayMessages() {
        return (from msg in this select new ArrayMessage(msg.TypeIm, msg)).ToList();
    }
}
