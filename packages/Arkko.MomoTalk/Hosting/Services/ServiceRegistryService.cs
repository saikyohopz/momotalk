using Microsoft.Extensions.DependencyInjection;

namespace Arkko.MomoTalk.Hosting.Services;

[Singleton]
public class ServiceRegistryService {
    private readonly IServiceCollection _services;

    public ServiceRegistryService(IServiceCollection services) {
        _services = services;
    }

    /// <summary>
    /// 检查服务是否已注册（包括非键控和键控服务）
    /// </summary>
    public bool IsRegistered(Type serviceType, object? key = null) {
        return _services.Any(d =>
            d.ServiceType == serviceType &&
            Equals(d.ServiceKey, key)
        );
    }

    /// <summary>
    /// 获取服务的注册信息（生命周期、是否键控等）
    /// </summary>
    public IEnumerable<ServiceInfo> GetServiceInfo(Type serviceType) {
        return _services
            .Where(d => d.ServiceType == serviceType)
            .Select(d => new ServiceInfo {
                ServiceType = d.ServiceType,
                ImplementationType = d.ImplementationType,
                Lifetime = d.Lifetime,
                IsKeyed = d.ServiceKey != null,
                Key = d.ServiceKey,
            });
    }
}
