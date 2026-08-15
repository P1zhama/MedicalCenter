using Authorization.Api.Interceptors;
using Authorization.Api.Services;
using Authorization.Application;
using Common.Api.Interceptors;
using Authorization.Infrastructure;
using Authorization.Infrastructure.Bootstrap;
using Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(new CompactJsonFormatter(), "Logs/bootstrap-log-.json", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Authorization Microservice...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<CorrelationIdInterceptor>();
        options.Interceptors.Add<GrpcExceptionInterceptor>();
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.MigrateAsync();

        var bootstrapAdminSeeder = scope.ServiceProvider.GetRequiredService<BootstrapAdminSeeder>();
        await bootstrapAdminSeeder.SeedAsync();
    }

    app.UseSerilogRequestLogging();

    app.MapGrpcService<AuthGrpcService>();
    app.MapGrpcService<AuthInternalGrpcService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Authorization microservice terminated unexpectedly");

    Environment.ExitCode = 1;
}
finally
{
    Log.Information("Shutting down Authorization...");
    Log.CloseAndFlush();
}
