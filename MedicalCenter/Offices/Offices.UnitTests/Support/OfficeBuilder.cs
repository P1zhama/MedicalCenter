using Common.Domain;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.Domain.ValueObjects;

namespace Offices.UnitTests.Support;

public sealed class OfficeBuilder
{
    public static readonly Guid DefaultId = new("0b9f3c52-4a8e-4d1f-9b6a-2f7c1e5d8a01");

    public static readonly Guid DefaultCreatorId = new("5d2a7e19-3c4b-4f6e-8a1d-9e0f2b3c4d02");

    public static readonly DateTimeOffset DefaultCreatedAt = new(2026, 1, 12, 9, 0, 0, TimeSpan.Zero);

    private OfficeStatus _status = OfficeStatus.Active;
    private long _version = 1;

    public OfficeBuilder WithStatus(OfficeStatus status)
    {
        _status = status;
        return this;
    }

    public OfficeBuilder WithVersion(long version)
    {
        _version = version;
        return this;
    }

    public Office Build()
        => Office.Restore(
            DefaultId,
            Address.Create("Москва", "Тверская", "12", "101"),
            "+74951234567",
            "/api/documents/7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e03",
            _status,
            _version,
            new AuditInfo(DefaultCreatorId, DefaultCreatedAt, null, null));
}
