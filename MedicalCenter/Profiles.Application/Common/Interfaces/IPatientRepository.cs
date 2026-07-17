
using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Common.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    // Поиск совпадения среди непривязанных профилей (AC-4, AC-9 в US-8):
    // весовые коэффициенты first/last/middle name = 5, дата рождения = 3, порог совпадения = 13
    Task<Patient?> GetBestMatchAsync(
        string firstName,
        string lastName,
        string? middleName,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken);

    Task AddAsync(Patient patient, CancellationToken cancellationToken);
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken);
}