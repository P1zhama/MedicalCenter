using Common.Abstractions.Eventing;
using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using Common.Infrastructure.Eventing;
using Common.Infrastructure.Providers;
using Common.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Common.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IGuidProvider, GuidProvider>();
        services.AddSingleton<IRandomProvider, RandomProvider>();

        services.AddScoped<CurrentUserProvider>();
        services.AddScoped<ICurrentUserProvider>(provider => provider.GetRequiredService<CurrentUserProvider>());

        services.AddScoped<IEventPublisher, EventPublisher>();

        return services;
    }
}
