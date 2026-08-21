using Appointments.Api.Protos;
using Common.Infrastructure;
using Documents.Application.Common.Interfaces;
using Documents.Application.Common.Settings;
using Documents.Infrastructure.Clients;
using Common.Infrastructure.Interceptors;
using Documents.Infrastructure.Maintenance;
using Profiles.Api.Protos;
using Documents.Infrastructure.Pdf;
using Documents.Infrastructure.Persistence;
using Documents.Infrastructure.Repositories;
using Documents.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Documents.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();

        services.AddDbContext<DocumentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        var minio = configuration.GetSection(MinioSettings.SectionName).Get<MinioSettings>()
            ?? throw new InvalidOperationException($"Section '{MinioSettings.SectionName}' is missing.");

        EnsureConfigured(minio);

        services.Configure<MinioSettings>(configuration.GetSection(MinioSettings.SectionName));

        services.AddSingleton<IMinioClient>(_ => new MinioClient()
            .WithEndpoint(minio.Endpoint)
            .WithCredentials(minio.AccessKey, minio.SecretKey)
            .WithSSL(minio.UseSsl)
            .Build());

        services.AddSingleton<IFileStorage, MinioFileStorage>();
        services.AddHostedService<MinioBucketInitializer>();

        services.AddScoped<IDocumentCommandRepository, DocumentCommandRepository>();
        services.AddScoped<IDocumentQueryRepository, DocumentQueryRepository>();
        services.AddScoped<IDocumentMaintenanceRepository, DocumentMaintenanceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<DocumentCleanupSettings>(configuration.GetSection(DocumentCleanupSettings.SectionName));
        services.AddHostedService<OrphanDocumentSweeper>();

        var pdf = configuration.GetSection(PdfSettings.SectionName).Get<PdfSettings>()
            ?? throw new InvalidOperationException($"Section '{PdfSettings.SectionName}' is missing.");

        PdfEnvironment.Configure(pdf);

        services.Configure<PdfSettings>(configuration.GetSection(PdfSettings.SectionName));
        services.AddSingleton<IPdfRenderer, AppointmentResultPdfRenderer>();

        services.AddHttpContextAccessor();
        services.AddTransient<TokenForwardingInterceptor>();

        services.AddGrpcClient<AppointmentsService.AppointmentsServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Appointments"] ?? "http://localhost:8006");
        }).AddInterceptor<TokenForwardingInterceptor>();

        services.AddGrpcClient<ProfilesService.ProfilesServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Profiles"] ?? "http://localhost:8001");
        }).AddInterceptor<TokenForwardingInterceptor>();

        services.AddScoped<IAppointmentResultClient, AppointmentResultClient>();
        services.AddScoped<IPhotoReferenceClient, PhotoReferenceClient>();

        services.AddMassTransit(bus =>
        {
            bus.AddEntityFrameworkOutbox<DocumentsDbContext>(outbox =>
            {
                outbox.UsePostgres();
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

                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("documents", false));
            });
        });

        return services;
    }

    private static void EnsureConfigured(MinioSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Endpoint))
            throw new InvalidOperationException("Object storage endpoint is not configured.");

        if (string.IsNullOrWhiteSpace(settings.AccessKey) || string.IsNullOrWhiteSpace(settings.SecretKey))
            throw new InvalidOperationException("Object storage credentials are not configured.");

        if (string.IsNullOrWhiteSpace(settings.Bucket))
            throw new InvalidOperationException("Object storage bucket is not configured.");
    }
}
