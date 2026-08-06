namespace Profiles.Domain.Constants;

public static class Permissions
{
    public const string CreateDoctor = "doctors:create";

    public const string EditDoctor = "doctors:edit";

    public const string ChangeDoctorStatus = "doctors:change-status";

    public const string ViewDoctors = "doctors:view";

    public const string CreateReceptionist = "receptionists:create";

    public const string EditReceptionist = "receptionists:edit";

    public const string DeleteReceptionist = "receptionists:delete";

    public const string ViewReceptionists = "receptionists:view";

    public const string CreatePatient = "patients:create";

    public const string EditPatient = "patients:edit";

    public const string DeletePatient = "patients:delete";

    public const string ViewPatients = "patients:view";
}
