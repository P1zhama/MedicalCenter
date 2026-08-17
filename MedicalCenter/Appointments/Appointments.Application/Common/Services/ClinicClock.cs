namespace Appointments.Application.Common.Services;

public sealed class ClinicClock
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _timeZone;

    public ClinicClock(TimeProvider timeProvider, TimeZoneInfo timeZone)
    {
        _timeProvider = timeProvider;
        _timeZone = timeZone;
    }

    public DateTimeOffset Now => TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _timeZone);

    public DateOnly Today => DateOnly.FromDateTime(Now.DateTime);

    public TimeOnly CurrentTime => TimeOnly.FromDateTime(Now.DateTime);
}
