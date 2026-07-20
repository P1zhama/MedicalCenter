using Authorization.API.Protos;
using Gateway.API.Interceptors;
using Gateway.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Offices.API.Protos;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Profiles.API.Protos;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.Net.Http;
using System.Text;

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

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<CorrelationIdClientInterceptor>();

    builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcClients:Authorization"] ?? "http://localhost:8000");
    }).AddInterceptor<CorrelationIdClientInterceptor>();

    builder.Services.AddGrpcClient<ProfilesService.ProfilesServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcClients:Profiles"] ?? "http://localhost:8001");
    }).AddInterceptor<CorrelationIdClientInterceptor>();

    builder.Services.AddGrpcClient<OfficesService.OfficesServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcClients:Offices"] ?? "http://localhost:8002");
    }).AddInterceptor<CorrelationIdClientInterceptor>();

    var jwtSection = builder.Configuration.GetSection("JwtSettings");
    var jwtSecretKey = jwtSection["SecretKey"];

    const string RoleClaimType = "role";
    const string SubjectClaimType = "sub";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey ?? string.Empty)),
                ValidateIssuer = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSection["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = RoleClaimType,
                NameClaimType = SubjectClaimType
            };
        });

    builder.Services.AddAuthorization();

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
    app.UseSerilogRequestLogging();

    app.UseCors(AllowedOriginsPolicy);

    app.UseRouting();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

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
