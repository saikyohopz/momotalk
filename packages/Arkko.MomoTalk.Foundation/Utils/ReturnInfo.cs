namespace Arkko.MomoTalk.Foundation.Utils;

/// <summary>
/// 方法返回值信息
/// </summary>
public record ReturnInfo(
    bool HasReturnValue,
    Type? ReturnType,
    bool IsAsync,
    bool IsTaskBased
);
