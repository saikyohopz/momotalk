using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Conversion;

public class ObImageToByteArrayMessageConverter : IMessageConverter<ObImage, byte[]?> {
    private readonly HttpClient _httpClient = new();

    public byte[]? ConvertMessage(ObImage message) {
        return message.Url == null ? null : _httpClient.GetByteArrayAsync(message.Url).GetAwaiter().GetResult();
    }
}

public class ObImageToBase64StringMessageConverter : IMessageConverter<ObImage, string?> {
    private readonly HttpClient _httpClient = new();

    public string? ConvertMessage(ObImage message) {
        return message.Url == null
            ? null
            : Convert.ToBase64String(
                _httpClient.GetByteArrayAsync(message.Url).GetAwaiter().GetResult()
            );
    }
}
