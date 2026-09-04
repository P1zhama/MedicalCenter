using Appointments.Application.Common.Dtos;

namespace Appointments.Application.Common.Services;

public static class PatientNames
{
    public static string Full(PatientSummaryDto? patient)
        => patient is null
            ? string.Empty
            : Full(patient.LastName, patient.FirstName, patient.MiddleName);

    public static string Full(string? lastName, string? firstName, string? middleName)
        => string.Join(' ', new[] { lastName, firstName, middleName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
