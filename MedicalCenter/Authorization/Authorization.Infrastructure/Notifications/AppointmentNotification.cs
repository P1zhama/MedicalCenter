namespace Authorization.Infrastructure.Notifications;

public sealed record AppointmentNotification(
    string Kind,
    DateTime AppointmentStart,
    string PatientFullName,
    string DoctorFullName,
    string ServiceName);

public static class AppointmentNotificationKinds
{
    public const string Approved = "Approved";

    public const string Cancelled = "Cancelled";

    public const string Rescheduled = "Rescheduled";

    public const string Reminder = "Reminder";
}
