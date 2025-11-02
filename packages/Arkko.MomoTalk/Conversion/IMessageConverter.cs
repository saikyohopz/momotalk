using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Conversion;

public interface IMessageConverter;

public interface IMessageConverter<in TMessage, out TOut> : IMessageConverter where TMessage : MessageBase {
    TOut ConvertMessage(TMessage message);
}
