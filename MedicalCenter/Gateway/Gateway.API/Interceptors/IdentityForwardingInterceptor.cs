using System.Linq;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;

namespace Gateway.API.Interceptors;

public sealed class IdentityForwardingInterceptor : Interceptor
{
    public const string UserIdHeader = "x-user-id";

    public const string RolesHeader = "x-user-roles";

    public const string PermissionsHeader = "x-user-permissions";

    private const string SubjectClaimType = "sub";
    private const string RoleClaimType = "role";
    private const string PermissionClaimType = "permission";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityForwardingInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return continuation(request, context);

        var headers = context.Options.Headers ?? new Metadata();

        var userId = user.FindFirst(SubjectClaimType)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            headers.Add(UserIdHeader, userId);

        var roles = user.FindAll(RoleClaimType).Select(claim => claim.Value).ToArray();
        if (roles.Length > 0)
            headers.Add(RolesHeader, string.Join(',', roles));

        var permissions = user.FindAll(PermissionClaimType).Select(claim => claim.Value).ToArray();
        if (permissions.Length > 0)
            headers.Add(PermissionsHeader, string.Join(',', permissions));

        var options = context.Options.WithHeaders(headers);

        var forwardedContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            options);

        return continuation(request, forwardedContext);
    }
}
