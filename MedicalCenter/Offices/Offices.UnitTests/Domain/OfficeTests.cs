using Common.Domain;
using Common.Domain.Exceptions;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.Domain.ValueObjects;
using Offices.UnitTests.Support;

namespace Offices.UnitTests.Domain;

public sealed class OfficeTests
{
    private const string PhoneNumber = "+74951234567";
    private const string PhotoUrl = "/api/documents/3e4f5a6b-7c8d-4e9f-8a0b-1c2d3e4f5a06";

    private static readonly Guid OfficeId = new("2c3d4e5f-6a7b-4c8d-9e0f-1a2b3c4d5e07");
    private static readonly Guid CreatorId = new("4a5b6c7d-8e9f-4a0b-9c1d-2e3f4a5b6c08");
    private static readonly Guid EditorId = new("6e7f8a9b-0c1d-4e2f-8a3b-4c5d6e7f8a09");
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 12, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedAt = new(2026, 2, 3, 14, 15, 0, TimeSpan.Zero);
    private static readonly Address MoscowAddress = Address.Create("Москва", "Тверская", "12", "101");
    private static readonly Address PetersburgAddress = Address.Create("Санкт-Петербург", "Невский проспект", "88", null);

    [Theory]
    [InlineData(OfficeStatus.Active)]
    [InlineData(OfficeStatus.Inactive)]
    public void Create_ValidData_ReturnsFirstVersionWithGivenValues(OfficeStatus status)
    {
        var office = Office.Create(OfficeId, MoscowAddress, PhoneNumber, PhotoUrl, status, CreatorId, CreatedAt);

        office.Id.Should().Be(OfficeId);
        office.Address.Should().Be(MoscowAddress);
        office.RegistryPhoneNumber.Should().Be(PhoneNumber);
        office.PhotoUrl.Should().Be(PhotoUrl);
        office.Status.Should().Be(status);
        office.Version.Should().Be(1);
        office.Audit.Should().Be(new AuditInfo(CreatorId, CreatedAt, null, null));
    }

    [Fact]
    public void Create_PhoneWithSurroundingSpaces_TrimsPhone()
    {
        var office = Office.Create(OfficeId, MoscowAddress, "  +74951234567 ", null, OfficeStatus.Active, CreatorId, CreatedAt);

        office.RegistryPhoneNumber.Should().Be(PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankPhone_ThrowsDomainException(string? phoneNumber)
    {
        var act = () => Office.Create(OfficeId, MoscowAddress, phoneNumber!, null, OfficeStatus.Active, CreatorId, CreatedAt);

        act.Should().Throw<DomainException>().WithMessage("registryPhoneNumber *");
    }

    [Theory]
    [InlineData(OfficeStatus.Active, true)]
    [InlineData(OfficeStatus.Inactive, false)]
    public void IsActive_ForStatus_ReturnsWhetherOfficeIsActive(OfficeStatus status, bool expected)
    {
        var office = new OfficeBuilder().WithStatus(status).Build();

        office.IsActive.Should().Be(expected);
    }

    [Theory]
    [InlineData(OfficeStatus.Active, OfficeStatus.Inactive, true)]
    [InlineData(OfficeStatus.Active, OfficeStatus.Active, false)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Inactive, false)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Active, false)]
    public void ChangeStatus_Transition_ReturnsTrueOnlyWhenActiveOfficeIsDeactivated(
        OfficeStatus from,
        OfficeStatus to,
        bool expectedDeactivated)
    {
        var office = new OfficeBuilder().WithStatus(from).Build();

        var deactivated = office.ChangeStatus(to, EditorId, UpdatedAt);

        deactivated.Should().Be(expectedDeactivated);
    }

    [Theory]
    [InlineData(OfficeStatus.Active, OfficeStatus.Inactive)]
    [InlineData(OfficeStatus.Active, OfficeStatus.Active)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Inactive)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Active)]
    public void ChangeStatus_AnyTransition_SetsStatusIncrementsVersionAndRecordsEditor(OfficeStatus from, OfficeStatus to)
    {
        var office = new OfficeBuilder().WithStatus(from).WithVersion(4).Build();

        office.ChangeStatus(to, EditorId, UpdatedAt);

        office.Status.Should().Be(to);
        office.Version.Should().Be(5);
        office.Audit.Should().Be(
            new AuditInfo(OfficeBuilder.DefaultCreatorId, OfficeBuilder.DefaultCreatedAt, EditorId, UpdatedAt));
    }

    [Theory]
    [InlineData(OfficeStatus.Active, OfficeStatus.Inactive, true)]
    [InlineData(OfficeStatus.Active, OfficeStatus.Active, false)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Inactive, false)]
    [InlineData(OfficeStatus.Inactive, OfficeStatus.Active, false)]
    public void Update_Transition_ReturnsTrueOnlyWhenActiveOfficeIsDeactivated(
        OfficeStatus from,
        OfficeStatus to,
        bool expectedDeactivated)
    {
        var office = new OfficeBuilder().WithStatus(from).Build();

        var deactivated = office.Update(PetersburgAddress, PhoneNumber, null, to, EditorId, UpdatedAt);

        deactivated.Should().Be(expectedDeactivated);
    }

    [Fact]
    public void Update_ValidData_ReplacesDetailsIncrementsVersionAndRecordsEditor()
    {
        var office = new OfficeBuilder().WithVersion(4).Build();

        office.Update(PetersburgAddress, " +78121234567 ", null, OfficeStatus.Inactive, EditorId, UpdatedAt);

        office.Address.Should().Be(PetersburgAddress);
        office.RegistryPhoneNumber.Should().Be("+78121234567");
        office.PhotoUrl.Should().BeNull();
        office.Status.Should().Be(OfficeStatus.Inactive);
        office.Version.Should().Be(5);
        office.Audit.Should().Be(
            new AuditInfo(OfficeBuilder.DefaultCreatorId, OfficeBuilder.DefaultCreatedAt, EditorId, UpdatedAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_BlankPhone_ThrowsAndLeavesOfficeUnchanged(string? phoneNumber)
    {
        var office = new OfficeBuilder().Build();

        var act = () => office.Update(PetersburgAddress, phoneNumber!, null, OfficeStatus.Inactive, EditorId, UpdatedAt);

        act.Should().Throw<DomainException>().WithMessage("registryPhoneNumber *");
        office.Should().BeEquivalentTo(new OfficeBuilder().Build(), options => options.ComparingByMembers<Office>());
    }

    [Fact]
    public void Restore_StoredState_KeepsEveryValueAsIs()
    {
        var audit = new AuditInfo(CreatorId, CreatedAt, EditorId, UpdatedAt);

        var office = Office.Restore(OfficeId, MoscowAddress, PhoneNumber, null, OfficeStatus.Inactive, 7, audit);

        office.Id.Should().Be(OfficeId);
        office.Address.Should().Be(MoscowAddress);
        office.RegistryPhoneNumber.Should().Be(PhoneNumber);
        office.PhotoUrl.Should().BeNull();
        office.Status.Should().Be(OfficeStatus.Inactive);
        office.Version.Should().Be(7);
        office.Audit.Should().Be(audit);
    }
}
