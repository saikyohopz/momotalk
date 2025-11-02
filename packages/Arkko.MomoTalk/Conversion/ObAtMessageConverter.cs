using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Conversion;

public class ObAtMessageConverter : IMessageConverter<ObAt, long> {
    public long ConvertMessage(ObAt message) {
        return message.UserId;
    }
}
