using Common.Abstractions.Security;
using Documents.Domain.Constants;

namespace Documents.Application.Common.Behaviors;

public static class PermissionCheck
{
    public static bool Has(CurrentUser user, string permission)
        => user.Permissions.Contains(permission)
           || user.Roles.Any(role => RolePermissions.Grants(role, permission));
}
