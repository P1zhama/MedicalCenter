using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Security;
using Authorization.Domain.Constants;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    public AuthorizationBehavior(
        ICurrentUserProvider currentUserProvider,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuthorizedRequest authorizedRequest)
            return await next();

        var user = _currentUserProvider.User;

        if (user is null || !user.IsAuthenticated)
        {
            _logger.LogWarning(
                "{RequestName} denied: no authenticated user for permission {Permission}",
                typeof(TRequest).Name,
                authorizedRequest.RequiredPermission);

            var unauthorized = new List<Error>
            {
                Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.")
            };

            return (dynamic)unauthorized;
        }

        if (!HasPermission(user, authorizedRequest.RequiredPermission))
        {
            _logger.LogWarning(
                "User {UserId} denied {Permission} for {RequestName}",
                user.Id,
                authorizedRequest.RequiredPermission,
                typeof(TRequest).Name);

            var forbidden = new List<Error>
            {
                Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.")
            };

            return (dynamic)forbidden;
        }

        return await next();
    }

    private static bool HasPermission(CurrentUser user, string permission)
        => user.Permissions.Contains(permission)
           || user.Roles.Any(role => RolePermissions.Grants(role, permission));
}
