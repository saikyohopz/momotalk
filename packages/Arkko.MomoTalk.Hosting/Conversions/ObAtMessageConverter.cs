using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Hosting.Conversions;

public class ObAtMessageConverter : IMessageConverter<ObAt, long> {
    public long ConvertMessage(ObAt message) {
        return message.UserId;
    }
}
