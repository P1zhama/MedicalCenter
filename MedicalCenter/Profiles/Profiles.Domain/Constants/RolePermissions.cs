namespace Profiles.Domain.Constants;

public static class RolePermissions
{
    private static readonly IReadOnlySet<string> Empty = new HashSet<string>();

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Map =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [Roles.Receptionist] = new HashSet<string>(StringComparer.Ordinal)
            {
                Permissions.CreateDoctor,
                Permissions.EditDoctor,
                Permissions.ChangeDoctorStatus,
                Permissions.ViewDoctors,
                Permissions.CreateReceptionist,
                Permissions.EditReceptionist,
                Permissions.DeleteReceptionist,
                Permissions.ViewReceptionists,
                Permissions.CreatePatient,
                Permissions.EditPatient,
                Permissions.DeletePatient,
                Permissions.ViewPatients
            },
            [Roles.Doctor] = new HashSet<string>(StringComparer.Ordinal)
            {
                Permissions.ViewPatients
            },
            [Roles.Patient] = Empty
        };

    public static bool Grants(string role, string permission)
        => Map.TryGetValue(role, out var permissions) && permissions.Contains(permission);

    public static IReadOnlySet<string> ForRole(string role)
        => Map.TryGetValue(role, out var permissions) ? permissions : Empty;
}
