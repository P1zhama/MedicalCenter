namespace Authorization.Domain.Constants;

public static class RolePermissions
{
    private static readonly IReadOnlySet<string> Empty = new HashSet<string>();

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Map =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [Roles.Receptionist] = new HashSet<string>(StringComparer.Ordinal)
            {
                Permissions.CreateWorkerAccount,
                Permissions.ManageAccountPermissions
            },
            [Roles.Doctor] = Empty,
            [Roles.Patient] = Empty
        };

    public static bool Grants(string role, string permission)
        => Map.TryGetValue(role, out var permissions) && permissions.Contains(permission);

    public static IReadOnlySet<string> ForRole(string role)
        => Map.TryGetValue(role, out var permissions) ? permissions : Empty;
}
