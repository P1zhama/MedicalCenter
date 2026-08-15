using Common.Domain.Exceptions;

namespace Appointments.Domain.Scheduling;

public sealed class WorkingSchedule
{
    private readonly IReadOnlySet<DayOfWeek> _workingDays;

    public WorkingSchedule(
        TimeOnly start,
        TimeOnly end,
        TimeOnly breakStart,
        TimeOnly breakEnd,
        int slotMinutes,
        IReadOnlySet<DayOfWeek> workingDays)
    {
        if (start >= end)
            throw new DomainException("Working day start must be earlier than its end.");

        if (breakStart >= breakEnd)
            throw new DomainException("Break start must be earlier than its end.");

        if (breakStart < start || breakEnd > end)
            throw new DomainException("Break must be inside the working day.");

        if (slotMinutes <= 0)
            throw new DomainException("Slot length must be greater than zero.");

        if (workingDays.Count == 0)
            throw new DomainException("At least one working day is required.");

        Start = start;
        End = end;
        BreakStart = breakStart;
        BreakEnd = breakEnd;
        SlotMinutes = slotMinutes;
        _workingDays = workingDays;
    }

    public TimeOnly Start { get; }

    public TimeOnly End { get; }

    public TimeOnly BreakStart { get; }

    public TimeOnly BreakEnd { get; }

    public int SlotMinutes { get; }

    public bool IsWorkingDay(DateOnly date) => _workingDays.Contains(date.DayOfWeek);

    public bool Fits(TimeOnly startTime, int durationMinutes)
    {
        if (durationMinutes <= 0)
            return false;

        if (startTime < Start)
            return false;

        var endTime = startTime.AddMinutes(durationMinutes);

        if (endTime > End)
            return false;

        if (startTime < BreakStart && endTime > BreakStart)
            return false;

        if (startTime >= BreakStart && startTime < BreakEnd)
            return false;

        return true;
    }

    public IEnumerable<TimeOnly> EnumerateStarts(int durationMinutes)
    {
        for (var time = Start; time.AddMinutes(durationMinutes) <= End; time = time.AddMinutes(SlotMinutes))
        {
            if (Fits(time, durationMinutes))
                yield return time;
        }
    }
}
