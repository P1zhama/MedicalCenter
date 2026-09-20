using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.Domain.ValueObjects;

namespace Offices.IntegrationTests.Support;

public static class TestOffice
{
    public static readonly Guid ReceptionistId = new("9a8b7c6d-5e4f-4a3b-8c2d-1e0f9a8b7c04");

    public static readonly DateTimeOffset CreatedAt = new(2026, 1, 12, 9, 0, 0, TimeSpan.Zero);

    public static readonly DateTimeOffset UpdatedAt = new(2026, 2, 3, 14, 15, 0, TimeSpan.Zero);

    public static Office Create(
        string city = "Москва",
        string street = "Тверская",
        string houseNumber = "12",
        string? officeNumber = "101",
        string? photoUrl = "/api/documents/7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e03",
        OfficeStatus status = OfficeStatus.Active)
        => Office.Create(
            Guid.NewGuid(),
            Address.Create(city, street, houseNumber, officeNumber),
            "+74951234567",
            photoUrl,
            status,
            ReceptionistId,
            CreatedAt);
}
