using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Conversion;

public class SelfMessageConverter<T> : IMessageConverter<T, T> where T : MessageBase {
    public T ConvertMessage(T message) {
        return message;
    }
}
