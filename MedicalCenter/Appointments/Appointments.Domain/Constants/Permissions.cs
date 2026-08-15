namespace Appointments.Domain.Constants;

public static class Permissions
{
    public const string CreateOwnAppointment = "appointments:create-own";

    public const string CreateAppointment = "appointments:create";

    public const string ViewOwnAppointments = "appointments:view-own";

    public const string ViewAppointments = "appointments:view";

    public const string ViewDoctorSchedule = "appointments:view-schedule";

    public const string RescheduleOwnAppointment = "appointments:reschedule-own";

    public const string RescheduleAppointment = "appointments:reschedule";

    public const string ApproveAppointment = "appointments:approve";

    public const string CancelAppointment = "appointments:cancel";

    public const string ViewResults = "results:view";

    public const string ManageResults = "results:manage";
}
