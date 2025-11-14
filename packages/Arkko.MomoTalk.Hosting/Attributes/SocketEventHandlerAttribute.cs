using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Attributes;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class SocketEventHandlerAttribute : Attribute;
