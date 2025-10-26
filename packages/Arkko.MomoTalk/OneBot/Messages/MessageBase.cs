using Arkko.MomoTalk.Utils;

namespace Arkko.MomoTalk.OneBot.Messages;

public abstract class MessageBase {
    internal abstract string TypeIm { get; }
    
    public abstract string ToContentString();

    public abstract string ToCqCodeString();

    public abstract object ToArrayMessageData();
}
