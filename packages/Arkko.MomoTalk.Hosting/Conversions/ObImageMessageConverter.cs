using Arkko.MomoTalk.Hosting.Services;
using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Hosting.Conversions;

public static class ObImageMessageConverters {
    public class ToByteArray(FileDownloadService fileDownloadService) : IMessageConverter<ObImage, byte[]?> {
        public byte[]? ConvertMessage(ObImage message) {
            return message.Url == null ? null : fileDownloadService.DownloadFile(message.Url, message.File);
        }
    }

    public class ToBase64String(FileDownloadService fileDownloadService) : IMessageConverter<ObImage, string?> {
        public string? ConvertMessage(ObImage message) {
            return message.Url == null
                ? null
                : Convert.ToBase64String(
                    fileDownloadService.DownloadFile(message.Url, message.File)
                );
        }
    }
}
