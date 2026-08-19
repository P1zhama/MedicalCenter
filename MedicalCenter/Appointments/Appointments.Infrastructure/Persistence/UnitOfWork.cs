using Appointments.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointments.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private const string ExclusionViolation = "23P01";

    private readonly AppointmentsDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(AppointmentsDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SaveOutcome> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return SaveOutcome.Success;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _logger.LogWarning(
                exception,
                "Concurrency conflict while saving changes; {EntryCount} entry(ies) were modified by another operation.",
                exception.Entries.Count);

            return SaveOutcome.ConcurrencyConflict;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: ExclusionViolation })
        {
            _logger.LogWarning(exception, "Appointment slot was taken by a concurrent request.");

            return SaveOutcome.SlotTaken;
        }
    }
}
