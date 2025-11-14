using Arkko.MomoTalk.Hosting.Attributes;
using Arkko.MomoTalk.OneBot.Protocol.Events;
using Arkko.MomoTalk.OneBot.Protocol.Messages;

namespace Arkko.MomoTalk.Example;

[Singleton]
public class EventService {
    [SocketEventHandler]
    private async static Task OnConnectEvent(MomoTalk momoTalk, EventConnect ev) {
        await SendMutsumi(momoTalk);
    }

    [SocketEventHandler]
    private async static Task OnPrivateMessageEvent(MomoTalk momoTalk, EventMessagePrivate ev) {
        if (ev.UserId == 1781176460) {
            // await ev.ReplyAsync(ev.Message);
            // await SendMutsumi(momoTalk);
        }
    }

    [MessageCommandMapping("echo")]
    public async static Task<MessageChain> CommandEcho(MomoTalk momoTalk, [RawParameterChain] MessageChain args) {
        await Task.Delay(1000);

        return args;
    }

    [MessageCommandMapping("hello")]
    public static string CommandHello(MomoTalk momoTalk, string name) {
        return $"hello, {name}";
    }

    [MessageCommandMapping("image")]
    public static byte[] CommandImage(MomoTalk momoTalk, byte[] image, int a) {
        return image;
    }

    private async static Task SendMutsumi(MomoTalk momoTalk) {
        await momoTalk.SendPrivateMessage(1781176460, MessageChain.Builder.Image(
            await File.ReadAllBytesAsync(@"D:\Pictures\表情包\mutsumi\271ED7569E449C84E33DD73A12AA3CA4.png")
        ).Build());
    }
}
