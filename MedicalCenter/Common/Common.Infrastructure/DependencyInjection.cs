using Common.Abstractions.Providers;
using Common.Infrastructure.Providers;
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

        return services;
    }
}
