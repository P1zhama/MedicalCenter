namespace Appointments.Application.Common.Settings;

public sealed class WorkingHoursSettings
{
    public const string SectionName = "WorkingHours";

    public TimeOnly Start { get; set; }

    public TimeOnly End { get; set; }

    public TimeOnly BreakStart { get; set; }

    public TimeOnly BreakEnd { get; set; }

    public int SlotMinutes { get; set; }

    public DayOfWeek[] WorkingDays { get; set; } = [];

    public int BookingHorizonDays { get; set; }
}
