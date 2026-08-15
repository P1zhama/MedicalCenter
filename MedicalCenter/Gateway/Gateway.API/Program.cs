using Authorization.Api.Protos;
using Gateway.Api.ErrorHandling;
using Gateway.Api.Interceptors;
using Gateway.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Offices.Api.Protos;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Profiles.Api.Protos;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Services.Api.Protos;
using System;
using System.Net.Http;

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

    builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcClients:Authorization"] ?? "http://localhost:8000");
    }).AddInterceptor<CorrelationIdClientInterceptor>().AddInterceptor<IdentityForwardingInterceptor>();

    builder.Services.AddGrpcClient<ProfilesService.ProfilesServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcClients:Profiles"] ?? "http://localhost:8001");
    }).AddInterceptor<CorrelationIdClientInterceptor>().AddInterceptor<IdentityForwardingInterceptor>();

    builder.Services.AddGrpcClient<OfficesService.OfficesServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcClients:Offices"] ?? "http://localhost:8004");
    }).AddInterceptor<CorrelationIdClientInterceptor>().AddInterceptor<IdentityForwardingInterceptor>();

    builder.Services.AddGrpcClient<ServicesService.ServicesServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcClients:Services"] ?? "http://localhost:8002");
    }).AddInterceptor<CorrelationIdClientInterceptor>().AddInterceptor<IdentityForwardingInterceptor>();

    var proxyBuilder = builder.Services.AddReverseProxy();
    proxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddResiliencePipeline("DefaultStrategy", (resilienceBuilder, context) =>
    {
        resilienceBuilder.AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        });

        resilienceBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(15)
        });
    });

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseCors(AllowedOriginsPolicy);

    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.MapReverseProxy(proxyPipeline =>
    {
        proxyPipeline.Use(async (context, next) =>
        {
            var proxyFeature = context.Features.Get<Yarp.ReverseProxy.Model.IReverseProxyFeature>();
            var routeValues = proxyFeature?.Route.Config.Metadata;

            if (routeValues != null && routeValues.TryGetValue("ResiliencePolicy", out var policyName))
            {
                var pipelineProvider = context.RequestServices.GetRequiredService<ResiliencePipelineProvider<string>>();
                var pipeline = pipelineProvider.GetPipeline(policyName);

                await pipeline.ExecuteAsync(async cancellationToken =>
                    await next(context), context.RequestAborted);
            }
            else
            { 
                await next(context);
            }
        });
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway API terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down Gateway API...");
    Log.CloseAndFlush();
}
