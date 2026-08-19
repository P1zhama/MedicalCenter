namespace Appointments.Application.Common.Interfaces;

public enum SaveOutcome
{
    Success = 1,
    ConcurrencyConflict = 2,
    SlotTaken = 3
}

public interface IUnitOfWork
{
    Task<SaveOutcome> SaveChangesAsync(CancellationToken cancellationToken = default);
}
