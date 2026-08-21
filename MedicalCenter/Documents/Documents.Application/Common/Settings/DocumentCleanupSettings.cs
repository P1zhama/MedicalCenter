namespace Documents.Application.Common.Settings;

public sealed class DocumentCleanupSettings
{
    public const string SectionName = "DocumentCleanup";

    public bool Enabled { get; set; }

    public int IntervalMinutes { get; set; }

    public int MinimumAgeHours { get; set; }

    public int RevalidateAfterDays { get; set; }

    public int BatchSize { get; set; }
}
