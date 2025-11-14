using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Attributes;

/// <summary>
/// 用于标定某个入参注入发送信息的用户所在的群 ID，
/// 该入参可以在任意位置标记为 nullable，且若不被标记为 nullable，则该指令将不处理私聊消息，
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[UsedImplicitly]
public class GroupIdAttribute : Attribute;
