using Microsoft.Extensions.DependencyInjection;

namespace Arkko.MomoTalk.Hosting;

public class ServiceInfo {
    public required Type ServiceType { get; set; }
    public Type? ImplementationType { get; set; }
    public ServiceLifetime Lifetime { get; set; }
    public bool IsKeyed { get; set; }
    public object? Key { get; set; }
}
