using System.Security.Claims;
using System.Text.Json;
using Common.Abstractions.Security;
using Common.Api.Authentication;
using Common.Infrastructure.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Common.Api.Middleware;

public sealed class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CurrentUserProvider currentUserProvider)
    {
        if (context.Items.TryGetValue(AuthFailureCodes.HttpContextItem, out var failure) && failure is string code)
        {
            await WriteUnauthorizedAsync(context, code);

            return;
        }

        var principal = context.User;

        if (principal.Identity?.IsAuthenticated == true)
            currentUserProvider.Set(ToCurrentUser(principal));

        await _next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string code)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";

        var payload = new
        {
            status = StatusCodes.Status401Unauthorized,
            title = "Unauthorized",
            detail = code
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private static CurrentUser ToCurrentUser(ClaimsPrincipal principal)
    {
        Guid? id = Guid.TryParse(principal.FindFirst(JwtClaimTypes.Subject)?.Value, out var parsed) ? parsed : null;

        Guid? profileId = Guid.TryParse(principal.FindFirst(JwtClaimTypes.ProfileId)?.Value, out var parsedProfile)
            ? parsedProfile
            : null;

        var roles = principal.FindAll(JwtClaimTypes.Role).Select(claim => claim.Value).ToArray();
        var permissions = principal.FindAll(JwtClaimTypes.Permission).Select(claim => claim.Value).ToArray();

        return new CurrentUser(id, profileId, roles, permissions);
    }
}

public static class CurrentUserMiddlewareExtensions
{
    public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app)
        => app.UseMiddleware<CurrentUserMiddleware>();
}
