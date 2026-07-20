namespace Authorization.Domain.Constants;

public static class Roles
{
    public const string Patient = "Patient";

    public const string Doctor = "Doctor";

    public const string Receptionist = "Receptionist";

    public const string Admin = "Admin";

    private static readonly HashSet<string> Known = new(StringComparer.Ordinal)
    {
        Patient,
        Doctor,
        Receptionist,
        Admin
    };

    public static bool IsKnown(string? role) => !string.IsNullOrWhiteSpace(role) && Known.Contains(role);
}
