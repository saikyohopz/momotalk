using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Attributes;

/// <summary>
/// 用于标定某个入参是含有空格的字符串，该入参必须放置于最后，
/// 若该入参没有默认值，指令中不能有 nullable 入参
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[UsedImplicitly]
public class VarStringAttribute : Attribute;
