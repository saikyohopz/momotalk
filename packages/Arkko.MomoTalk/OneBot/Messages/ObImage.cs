using Arkko.MomoTalk.Utils;

namespace Arkko.MomoTalk.OneBot.Messages;

public class ObImage : MessageBase {
    internal override string TypeIm => "image";

    /// <summary>
    /// 文件名或路径
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// 图片类型，普通图片为null，闪照为flash
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 0=图片 1=动画表情
    /// </summary>
    public int SubType { get; set; }

    /// <summary>
    /// 图片链接，只有接收时不为 null，发送时只能为 null
    /// </summary>
    public string? Url { get; set; }

    // todo type, cache, proxy, timeout

    public ObImage(byte[] bytes) {
        File = "base64://" + Convert.ToBase64String(bytes);
    }

    internal ObImage() { }

    public override string ToContentString() {
        return SubType switch {
            1 => "[动画表情]",
            _ => "[图片]",
        };
    }

    public override string ToCqCodeString() {
        return CqUtils.BuildCqCodeString(TypeIm, Dict.Of<string, object>(
            ("file", File)
        ));
    }

    public override object ToArrayMessageData() {
        return new {
            File,
        };
    }
}
