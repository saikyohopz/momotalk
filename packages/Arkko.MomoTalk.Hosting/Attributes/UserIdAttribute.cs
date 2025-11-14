using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Attributes;

/// <summary>
/// 用于标定某个入参注入发送信息的用户 ID
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[UsedImplicitly]
public class UserIdAttribute : Attribute;
