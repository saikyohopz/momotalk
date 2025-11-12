using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting.Attributes;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class SingletonAttribute : Attribute;
