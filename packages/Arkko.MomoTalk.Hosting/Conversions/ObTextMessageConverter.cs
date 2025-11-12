using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Hosting.Conversions;

public class ObTextMessageConverter<TOut> : IMessageConverter<ObText, TOut> {
    public TOut ConvertMessage(ObText message) {
        Type outType = typeof(TOut);

        if (outType.IsEnum) {
            if (Enum.TryParse(outType, message.Text, true, out object? converted)) {
                return (TOut)converted;
            }

            throw new InvalidCastException("bad enum value");
        }

        return (TOut)Convert.ChangeType(message.Text, typeof(TOut));
    }
}
