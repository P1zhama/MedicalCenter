using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Profiles.Application.Common.Behaviors;
using System.Reflection;

namespace Profiles.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Валидаторы команд + пайплайн, прогоняющий их перед обработчиками
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}