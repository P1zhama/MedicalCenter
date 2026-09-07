using Common.Abstractions.Paging;
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

    public async Task<PagedResult<DoctorCardDto>> GetActiveCardsAsync(
        DoctorFilter filter,
        int currentYear,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(AtWorkOnly(), filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(doctor => doctor.LastName)
            .ThenBy(doctor => doctor.FirstName)
            .ThenBy(doctor => doctor.Id)
            .Skip(PageRequest.Skip(page, pageSize))
            .Take(pageSize)
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

        return new PagedResult<DoctorCardDto>(items, page, pageSize, totalCount);
    }

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

    public async Task<PagedResult<DoctorListItemDto>> SearchAsync(
        DoctorFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(_context.Doctors.AsNoTracking(), filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(doctor => doctor.LastName)
            .ThenBy(doctor => doctor.FirstName)
            .ThenBy(doctor => doctor.Id)
            .Skip(PageRequest.Skip(page, pageSize))
            .Take(pageSize)
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

        return new PagedResult<DoctorListItemDto>(items, page, pageSize, totalCount);
    }

    public Task<DoctorDto?> GetByIdAsync(Guid id, int currentYear, CancellationToken cancellationToken = default)
        => Project(_context.Doctors.AsNoTracking().Where(doctor => doctor.Id == id), currentYear)
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task<DoctorDto?> GetByAccountIdAsync(
        Guid accountId,
        int currentYear,
        CancellationToken cancellationToken = default)
        => Project(_context.Doctors.AsNoTracking().Where(doctor => doctor.AccountId == accountId), currentYear)
            .FirstOrDefaultAsync(cancellationToken)!;

    public async Task<DoctorForAppointmentDto?> GetForAppointmentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var atWork = DoctorStatus.AtWork.ToString();

        return await _context.Doctors
            .AsNoTracking()
            .Where(doctor => doctor.Id == id)
            .Select(doctor => new DoctorForAppointmentDto(
                doctor.Id,
                doctor.SpecializationId,
                doctor.OfficeId,
                doctor.Status == atWork))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetAtWorkIdsAsync(
        Guid specializationId,
        Guid? officeId,
        CancellationToken cancellationToken = default)
    {
        var query = AtWorkOnly().Where(doctor => doctor.SpecializationId == specializationId);

        if (officeId.HasValue)
            query = query.Where(doctor => doctor.OfficeId == officeId.Value);

        return await query
            .Select(doctor => doctor.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DoctorSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await _context.Doctors
            .AsNoTracking()
            .Where(doctor => ids.Contains(doctor.Id))
            .Select(doctor => new DoctorSummaryDto(
                doctor.Id,
                doctor.FirstName,
                doctor.LastName,
                doctor.MiddleName))
            .ToListAsync(cancellationToken);
    }

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
