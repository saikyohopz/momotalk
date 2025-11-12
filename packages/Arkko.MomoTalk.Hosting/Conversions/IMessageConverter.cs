using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Hosting.Conversions;

public interface IMessageConverter {
    Type InType { get; }
    Type OutType { get; }

    object? ConvertMessage(object message);
}

public interface IMessageConverter<in TMessage, out TOut> : IMessageConverter where TMessage : MessageBase {
    Type IMessageConverter.InType => typeof(TMessage);

    Type IMessageConverter.OutType => typeof(TOut);

    object? IMessageConverter.ConvertMessage(object message) {
        return ConvertMessage((TMessage)message);
    }

    TOut ConvertMessage(TMessage message);
}
