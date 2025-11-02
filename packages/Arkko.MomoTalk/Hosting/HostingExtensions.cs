using Arkko.MomoTalk.Foundation.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Arkko.MomoTalk.Hosting;

public static class HostingExtensions {
    public static IServiceCollection AddScopedByAttribute(this IServiceCollection builder) {
        return builder.DoSomethingByAttribute<ScopedAttribute>(t => builder.AddScoped(t));
    }
    
    public static IServiceCollection AddSingletonByAttribute(this IServiceCollection builder) {
        return builder.DoSomethingByAttribute<SingletonAttribute>(t => builder.AddSingleton(t));
    }

    private static IServiceCollection DoSomethingByAttribute<TAttribute>(
        this IServiceCollection builder, Action<Type> typeAction
    ) where TAttribute : Attribute {
        IEnumerable<Type> types = ReflectionUtils.FindTypesWithAttribute<TAttribute>();

        foreach (Type type in types) {
            typeAction.Invoke(type);
        }

        return builder;
    }
    
    public static bool IsRegistered(this IServiceCollection services, Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);

        return services.Any(descriptor => descriptor.ServiceType == serviceType);
    }
}
