using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Common;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class SocketEventHandlerAttribute : Attribute;
