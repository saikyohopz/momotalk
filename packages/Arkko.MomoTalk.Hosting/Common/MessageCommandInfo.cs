namespace Arkko.MomoTalk.Hosting.Common;

public struct MessageCommandInfo {
    /// <summary>
    /// 调用方式
    /// </summary>
    public string[] Aliases { get; set; }

    /// <summary>
    /// 指令描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 使用示例
    /// </summary>
    public string Example { get; set; }

    /// <summary>
    /// 是否从帮助列表中隐藏
    /// </summary>
    public bool HiddenFromHelp { get; set; }

    public MessageCommandInfo(
        string[] aliases, string description = "", string example = "", bool hiddenFromHelp = false
    ) {
        Aliases = aliases;
        Description = description;
        Example = example;
        HiddenFromHelp = hiddenFromHelp;
    }
}
