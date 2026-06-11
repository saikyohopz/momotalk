namespace Arkko.MomoTalk.Hosting.Enums;

public enum ForLoopAction {
    /// <summary>
    /// 进入下一个循环
    /// </summary>
    Continue,

    /// <summary>
    /// 中止循环
    /// </summary>
    Break,

    /// <summary>
    /// 函数返回
    /// </summary>
    Return,

    /// <summary>
    /// 执行后续代码
    /// </summary>
    DoNothing,
}
