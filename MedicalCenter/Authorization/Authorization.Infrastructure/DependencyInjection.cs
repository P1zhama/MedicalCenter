using Authorization.Application.Common.Interfaces;
using Authorization.Infrastructure.Authentication;
using Authorization.Infrastructure.Bootstrap;
using Authorization.Infrastructure.Messaging;
using Authorization.Infrastructure.Notifications;
using Authorization.Infrastructure.Persistence;
using Authorization.Infrastructure.Repositories;
using Authorization.Infrastructure.Services;
using System.Security.Cryptography;
using Common.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Authorization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException($"Section '{JwtSettings.SectionName}' is missing.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton(CreateSigningCredentials(jwtSettings.PrivateKey));
        services.Configure<EmailConfirmationSettings>(configuration.GetSection(EmailConfirmationSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<BootstrapAdminSettings>(configuration.GetSection(BootstrapAdminSettings.SectionName));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ITokenHashGenerator, TokenHashGenerator>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddSingleton<IEmailConfirmationTokenGenerator, EmailConfirmationTokenGenerator>();
        services.AddSingleton<IPasswordGenerator, PasswordGenerator>();
        services.AddScoped<IEmailSender, MailKitEmailSender>();
        services.AddScoped<BootstrapAdminSeeder>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<AccountConfirmationRequestedConsumer>();
            bus.AddConsumer<WorkerCredentialsIssuedConsumer>();
            bus.AddConsumer<ProfileLinkedToAccountEventConsumer>();
            bus.AddConsumer<WorkerDeactivatedEventConsumer>();
            bus.AddConsumer<WorkerReactivatedEventConsumer>();

            bus.AddEntityFrameworkOutbox<AuthDbContext>(outbox => 
            {
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMqSettings");

                cfg.Host(rabbit["Host"] ?? "rabbitmq", rabbit["VirtualHost"] ?? "/", host =>
                {
                    host.Username(rabbit["Username"] ?? "guest");
                    host.Password(rabbit["Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("authorization", false));
            });
        });

        return services;
    }

    private static SigningCredentials CreateSigningCredentials(string privateKey)
    {
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new InvalidOperationException("JWT private key is not configured.");

        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

        return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
    }
}
