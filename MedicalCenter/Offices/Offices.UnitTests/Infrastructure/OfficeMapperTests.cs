using Common.Domain;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.Domain.ValueObjects;
using Offices.Infrastructure.Persistence;

namespace Offices.UnitTests.Infrastructure;

public sealed class OfficeMapperTests
{
    private const string PhotoUrl = "/api/documents/0f1a2b3c-4d5e-4f6a-8b7c-9d0e1f2a3b26";

    private static readonly Guid OfficeId = new("1b2c3d4e-5f6a-4b7c-9d8e-0f1a2b3c4d27");
    private static readonly Guid CreatorId = new("2c3d4e5f-6a7b-4c8d-8e9f-1a2b3c4d5e28");
    private static readonly Guid EditorId = new("3d4e5f6a-7b8c-4d9e-9f0a-2b3c4d5e6f29");
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 12, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedAt = new(2026, 2, 3, 14, 15, 0, TimeSpan.Zero);

    private static Office StoredOffice(OfficeStatus status, string? officeNumber, string? photoUrl, AuditInfo audit)
        => Office.Restore(
            OfficeId,
            Address.Create("Москва", "Тверская", "12", officeNumber),
            "+74951234567",
            photoUrl,
            status,
            3,
            audit);

    private static OfficeDocument StoredDocument(string status) => new()
    {
        Id = OfficeId,
        City = "Москва",
        Street = "Тверская",
        HouseNumber = "12",
        OfficeNumber = null,
        RegistryPhoneNumber = "+74951234567",
        PhotoUrl = null,
        Status = status,
        Version = 3,
        CreatedBy = CreatorId,
        CreatedAt = CreatedAt,
        UpdatedBy = null,
        UpdatedAt = null
    };

    [Fact]
    public void ToDocument_Office_CopiesEveryFieldAndStoresStatusName()
    {
        var office = StoredOffice(
            OfficeStatus.Inactive,
            "101",
            PhotoUrl,
            new AuditInfo(CreatorId, CreatedAt, EditorId, UpdatedAt));

        var document = office.ToDocument();

        document.Should().BeEquivalentTo(new OfficeDocument
        {
            Id = OfficeId,
            City = "Москва",
            Street = "Тверская",
            HouseNumber = "12",
            OfficeNumber = "101",
            RegistryPhoneNumber = "+74951234567",
            PhotoUrl = PhotoUrl,
            Status = "Inactive",
            Version = 3,
            CreatedBy = CreatorId,
            CreatedAt = CreatedAt,
            UpdatedBy = EditorId,
            UpdatedAt = UpdatedAt
        });
    }

    [Fact]
    public void ToDomain_Document_RestoresEveryField()
    {
        var office = StoredDocument("Active").ToDomain();

        office.Id.Should().Be(OfficeId);
        office.Address.Should().Be(Address.Create("Москва", "Тверская", "12", null));
        office.RegistryPhoneNumber.Should().Be("+74951234567");
        office.PhotoUrl.Should().BeNull();
        office.Status.Should().Be(OfficeStatus.Active);
        office.Version.Should().Be(3);
        office.Audit.Should().Be(new AuditInfo(CreatorId, CreatedAt, null, null));
    }

    [Theory]
    [InlineData(OfficeStatus.Active)]
    [InlineData(OfficeStatus.Inactive)]
    public void ToDocumentThenToDomain_AnyOffice_RestoresSameState(OfficeStatus status)
    {
        var office = StoredOffice(status, "101", PhotoUrl, new AuditInfo(CreatorId, CreatedAt, EditorId, UpdatedAt));

        var restored = office.ToDocument().ToDomain();

        restored.Should().BeEquivalentTo(office, options => options.ComparingByMembers<Office>());
    }

    [Theory]
    [InlineData("Closed")]
    [InlineData("active")]
    [InlineData("")]
    public void ToDomain_UnknownStatusName_Throws(string status)
    {
        var document = StoredDocument(status);

        var act = () => document.ToDomain();

        act.Should().Throw<ArgumentException>();
    }
}
