using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Common;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class MessageCommandMappingAttribute : Attribute {
    public MessageCommandMappingAttribute(string alias) {
        Aliases = [alias];
    }

    public MessageCommandMappingAttribute(
        string[] aliases, string description = "", string example = "", bool hiddenFromHelp = false
    ) {
        Aliases = aliases;
        Description = description;
        Example = example;
        HiddenFromHelp = hiddenFromHelp;
    }

    /// <summary>
    /// 调用方式
    /// </summary>
    public string[] Aliases { get; set; }

    /// <summary>
    /// 指令描述
    /// </summary>
    public string Description { get; set; } = "没有描述";

    /// <summary>
    /// 使用示例
    /// </summary>
    public string Example { get; set; } = string.Empty;

    /// <summary>
    /// 是否从帮助列表中隐藏
    /// </summary>
    public bool HiddenFromHelp { get; set; }
}
