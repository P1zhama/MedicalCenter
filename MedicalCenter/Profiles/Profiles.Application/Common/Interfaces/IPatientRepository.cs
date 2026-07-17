
using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Common.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Patient?> GetBestMatchAsync(
        string firstName,
        string lastName,
        string? middleName,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken);

    Task AddAsync(Patient patient, CancellationToken cancellationToken);
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken);
}