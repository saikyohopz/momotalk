using Arkko.MomoTalk.Foundation.Utils;
using Arkko.MomoTalk.Hosting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arkko.MomoTalk.Hosting;

public static class HostingExtensions {
    public static IHostApplicationBuilder UseMomoTalk(
        this IHostApplicationBuilder builder, params MomoTalkConfig[] configs
    ) {
        builder.Services
            .AddScopedByAttribute()
            .AddSingletonByAttribute()
            .AddHostedService<MomoTalkHostedService>()
            .AddSingleton(sp => {
                ILoggerFactory loggerFactory = sp.GetService<ILoggerFactory>() ?? new NullLoggerFactory();
                BotCollectionService botCollectionService = new(loggerFactory, sp);
                botCollectionService.CreateBot(configs);
                return botCollectionService;
            });
        
        return builder;
    }

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
}
