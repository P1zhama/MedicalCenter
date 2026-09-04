namespace Appointments.Application.Common.Settings;

public sealed class ReminderSettings
{
    public const string SectionName = "Reminders";

    public bool Enabled { get; set; }

    public int IntervalMinutes { get; set; }

    public int BatchSize { get; set; }
}
