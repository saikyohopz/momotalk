using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
[UsedImplicitly]
public class RawParameterChainAttribute : Attribute;
