using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain.Enums;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Entities;

namespace Profiles.Infrastructure.Repositories;

public sealed class DoctorQueryRepository : IDoctorQueryRepository
{
    private readonly ProfilesDbContext _context;

    public DoctorQueryRepository(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DoctorCardDto>> GetActiveCardsAsync(
        DoctorFilter filter,
        int currentYear,
        CancellationToken cancellationToken = default)
        => await ApplyFilter(AtWorkOnly(), filter)
            .OrderBy(doctor => doctor.LastName)
            .ThenBy(doctor => doctor.FirstName)
            .Select(doctor => new DoctorCardDto(
                doctor.Id,
                doctor.PhotoUrl,
                doctor.FirstName,
                doctor.LastName,
                doctor.MiddleName,
                doctor.SpecializationId,
                doctor.OfficeId,
                currentYear - doctor.CareerStartYear + 1))
            .ToListAsync(cancellationToken);

    public Task<DoctorCardDto?> GetActiveCardByIdAsync(
        Guid id,
        int currentYear,
        CancellationToken cancellationToken = default)
        => AtWorkOnly()
            .Where(doctor => doctor.Id == id)
            .Select(doctor => new DoctorCardDto(
                doctor.Id,
                doctor.PhotoUrl,
                doctor.FirstName,
                doctor.LastName,
                doctor.MiddleName,
                doctor.SpecializationId,
                doctor.OfficeId,
                currentYear - doctor.CareerStartYear + 1))
            .FirstOrDefaultAsync(cancellationToken)!;

    public async Task<IReadOnlyList<DoctorListItemDto>> SearchAsync(
        DoctorFilter filter,
        CancellationToken cancellationToken = default)
        => await ApplyFilter(_context.Doctors.AsNoTracking(), filter)
            .OrderBy(doctor => doctor.LastName)
            .ThenBy(doctor => doctor.FirstName)
            .Select(doctor => new DoctorListItemDto(
                doctor.Id,
                doctor.FirstName,
                doctor.LastName,
                doctor.MiddleName,
                doctor.DateOfBirth,
                doctor.SpecializationId,
                doctor.OfficeId,
                doctor.Status))
            .ToListAsync(cancellationToken);

    public Task<DoctorDto?> GetByIdAsync(Guid id, int currentYear, CancellationToken cancellationToken = default)
        => Project(_context.Doctors.AsNoTracking().Where(doctor => doctor.Id == id), currentYear)
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task<DoctorDto?> GetByAccountIdAsync(
        Guid accountId,
        int currentYear,
        CancellationToken cancellationToken = default)
        => Project(_context.Doctors.AsNoTracking().Where(doctor => doctor.AccountId == accountId), currentYear)
            .FirstOrDefaultAsync(cancellationToken)!;

    private IQueryable<DoctorEntity> AtWorkOnly()
    {
        var atWork = DoctorStatus.AtWork.ToString();

        return _context.Doctors.AsNoTracking().Where(doctor => doctor.Status == atWork);
    }

    private static IQueryable<DoctorEntity> ApplyFilter(IQueryable<DoctorEntity> query, DoctorFilter filter)
    {
        if (filter.SpecializationId.HasValue)
            query = query.Where(doctor => doctor.SpecializationId == filter.SpecializationId.Value);

        if (filter.OfficeId.HasValue)
            query = query.Where(doctor => doctor.OfficeId == filter.OfficeId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            query = query.Where(doctor =>
                EF.Functions.Like(doctor.FirstName, $"%{search}%")
                || EF.Functions.Like(doctor.LastName, $"%{search}%")
                || (doctor.MiddleName != null && EF.Functions.Like(doctor.MiddleName, $"%{search}%")));
        }

        return query;
    }

    private static IQueryable<DoctorDto> Project(IQueryable<DoctorEntity> query, int currentYear)
        => query.Select(doctor => new DoctorDto(
            doctor.Id,
            doctor.PhotoUrl,
            doctor.FirstName,
            doctor.LastName,
            doctor.MiddleName,
            doctor.DateOfBirth,
            doctor.SpecializationId,
            doctor.OfficeId,
            doctor.CareerStartYear,
            currentYear - doctor.CareerStartYear + 1,
            doctor.Status));
}
