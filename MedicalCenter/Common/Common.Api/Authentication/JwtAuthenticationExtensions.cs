using System.Security.Cryptography;
using Common.Abstractions.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common.Api.Authentication;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtValidationSettings.SectionName).Get<JwtValidationSettings>()
            ?? throw new InvalidOperationException($"Section '{JwtValidationSettings.SectionName}' is missing.");

        if (string.IsNullOrWhiteSpace(settings.PublicKey))
            throw new InvalidOperationException("JWT public key is not configured.");

        var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(settings.PublicKey), out _);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),
                    RoleClaimType = JwtClaimTypes.Role,
                    NameClaimType = JwtClaimTypes.Subject
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.HttpContext.Items[AuthFailureCodes.HttpContextItem] =
                            context.Exception is SecurityTokenExpiredException
                                ? AuthFailureCodes.TokenExpired
                                : AuthFailureCodes.TokenInvalid;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
