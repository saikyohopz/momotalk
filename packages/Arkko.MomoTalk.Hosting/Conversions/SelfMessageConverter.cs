using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Hosting.Conversions;

public class SelfMessageConverter<T> : IMessageConverter<T, T> where T : MessageBase {
    public T ConvertMessage(T message) {
        return message;
    }
}
