using Common.Api.Interceptors;
using Microsoft.EntityFrameworkCore;
using Profiles.Api.Interceptors;
using Profiles.Api.Services;
using Profiles.Application;
using Profiles.Infrastructure;
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
    Log.Information("Starting Profiles Microservice...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));


    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<CorrelationIdInterceptor>();
        options.Interceptors.Add<UserContextInterceptor>();
        options.Interceptors.Add<GrpcExceptionInterceptor>();
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapGrpcService<ProfilesGrpcService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<Profiles.Infrastructure.Persistence.ProfilesDbContext>();

            if (context.Database.IsSqlServer())
            {
                context.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while performing a database migration.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Profiles microservice terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
