using System.Security.Claims;
using Common.Abstractions.Security;

namespace Common.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAuthenticated(this ClaimsPrincipal principal)
        => principal.Identity?.IsAuthenticated == true;

    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal)
    {
        Guid? id = Guid.TryParse(principal.FindFirst(JwtClaimTypes.Subject)?.Value, out var parsed)
            ? parsed
            : null;

        Guid? profileId = Guid.TryParse(principal.FindFirst(JwtClaimTypes.ProfileId)?.Value, out var parsedProfile)
            ? parsedProfile
            : null;

        var roles = principal.FindAll(JwtClaimTypes.Role).Select(claim => claim.Value).ToArray();
        var permissions = principal.FindAll(JwtClaimTypes.Permission).Select(claim => claim.Value).ToArray();

        return new CurrentUser(id, profileId, roles, permissions);
    }
}
