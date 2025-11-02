using JetBrains.Annotations;

namespace Arkko.MomoTalk.Common;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class SocketEventHandlerAttribute : Attribute;
