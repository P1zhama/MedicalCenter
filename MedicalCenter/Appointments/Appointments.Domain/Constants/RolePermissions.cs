namespace Appointments.Domain.Constants;

public static class RolePermissions
{
    private static readonly IReadOnlySet<string> Empty = new HashSet<string>();

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Map =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [Roles.Receptionist] = new HashSet<string>(StringComparer.Ordinal)
            {
                Permissions.CreateAppointment,
                Permissions.ViewAppointments,
                Permissions.RescheduleAppointment,
                Permissions.ApproveAppointment,
                Permissions.CancelAppointment
            },
            [Roles.Doctor] = new HashSet<string>(StringComparer.Ordinal)
            {
                Permissions.ViewDoctorSchedule,
                Permissions.ViewResults,
                Permissions.ManageResults
            },
            [Roles.Patient] = new HashSet<string>(StringComparer.Ordinal)
            {
                Permissions.CreateOwnAppointment,
                Permissions.ViewOwnAppointments,
                Permissions.RescheduleOwnAppointment,
                Permissions.ViewResults
            }
        };

    public static bool Grants(string role, string permission)
        => Map.TryGetValue(role, out var permissions) && permissions.Contains(permission);

    public static IReadOnlySet<string> ForRole(string role)
        => Map.TryGetValue(role, out var permissions) ? permissions : Empty;
}
