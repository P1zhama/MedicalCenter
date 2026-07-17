using Authorization.Application.Common.Configurations;
using Authorization.Application.Common.Interfaces;
using Authorization.Application.EventConsumers;
using Authorization.Infrastructure.EventConsumers;
using Authorization.Infrastructure.Persistence;
using Authorization.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Authorization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddTransient<IJwtTokenService, JwtTokenService>();
        services.AddTransient<IEmailService, EmailService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ProfileLinkedEventConsumer>();
            x.AddConsumer<EmailDeliveryConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitSettings = configuration.GetSection("RabbitMqSettings");

                cfg.Host(rabbitSettings["Host"] ?? "rabbitmq", rabbitSettings["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbitSettings["Username"] ?? "guest");
                    h.Password(rabbitSettings["Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("authorization-profile-linked-queue", e =>
                {
                    e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
                    e.ConfigureConsumer<ProfileLinkedEventConsumer>(context);
                });

                cfg.ReceiveEndpoint("authorization-email-delivery-queue", e =>
                {
                    e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
                    e.ConfigureConsumer<EmailDeliveryConsumer>(context);
                });

                // ConfigureEndpoints здесь не нужен: оба консьюмера уже привязаны к именованным
                // очередям выше со своими политиками ретраев. Автоконфигурация лишь создала бы
                // дублирующие эндпоинты по умолчанию
            });
        });

        return services;
    }
}