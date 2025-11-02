using JetBrains.Annotations;

namespace Arkko.MomoTalk.Hosting;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class SingletonAttribute : Attribute;
