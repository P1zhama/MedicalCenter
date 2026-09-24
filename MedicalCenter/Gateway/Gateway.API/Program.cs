using Appointments.Api.Protos;
using Authorization.Api.Protos;
using Gateway.Api.ErrorHandling;
using Gateway.Api.GrpcClients;
using Gateway.Api.Interceptors;
using Gateway.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Offices.Api.Protos;
using Profiles.Api.Protos;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Services.Api.Protos;
using System;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(new CompactJsonFormatter(), "Logs/bootstrap-log-.json", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Gateway API...");

    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    const string AllowedOriginsPolicy = "FrontendOrigins";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(AllowedOriginsPolicy, policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    builder.Services.AddControllers();

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GrpcExceptionHandler>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<CorrelationIdClientInterceptor>();
    builder.Services.AddTransient<IdentityForwardingInterceptor>();
    builder.Services.AddTransient<DefaultDeadlineInterceptor>();

    builder.Services.AddDownstreamGrpcClient<AuthService.AuthServiceClient>(
        builder.Configuration["GrpcClients:Authorization"] ?? "http://localhost:8000",
        AuthService.Descriptor);

    builder.Services.AddDownstreamGrpcClient<ProfilesService.ProfilesServiceClient>(
        builder.Configuration["GrpcClients:Profiles"] ?? "http://localhost:8001",
        ProfilesService.Descriptor);

    builder.Services.AddDownstreamGrpcClient<OfficesService.OfficesServiceClient>(
        builder.Configuration["GrpcClients:Offices"] ?? "http://localhost:8004",
        OfficesService.Descriptor);

    builder.Services.AddDownstreamGrpcClient<ServicesService.ServicesServiceClient>(
        builder.Configuration["GrpcClients:Services"] ?? "http://localhost:8002",
        ServicesService.Descriptor);

    builder.Services.AddDownstreamGrpcClient<AppointmentsService.AppointmentsServiceClient>(
        builder.Configuration["GrpcClients:Appointments"] ?? "http://localhost:8006",
        AppointmentsService.Descriptor);

    var proxyBuilder = builder.Services.AddReverseProxy();
    proxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseCors(AllowedOriginsPolicy);

    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.MapReverseProxy();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway API terminated unexpectedly");

    Environment.ExitCode = 1;
}
finally
{
    Log.Information("Shutting down Gateway API...");
    Log.CloseAndFlush();
}
