using Offices.Domain.Constants;

namespace Offices.UnitTests.Domain;

public sealed class RolePermissionsTests
{
    private static readonly string[] OfficePermissions =
    [
        Permissions.CreateOffice,
        Permissions.EditOffice,
        Permissions.ChangeOfficeStatus,
        Permissions.ViewOffices
    ];

    [Theory]
    [InlineData(Permissions.CreateOffice)]
    [InlineData(Permissions.EditOffice)]
    [InlineData(Permissions.ChangeOfficeStatus)]
    [InlineData(Permissions.ViewOffices)]
    public void Grants_ReceptionistWithOfficePermission_ReturnsTrue(string permission)
    {
        var granted = RolePermissions.Grants(Roles.Receptionist, permission);

        granted.Should().BeTrue();
    }

    [Fact]
    public void Grants_ReceptionistWithUnknownPermission_ReturnsFalse()
    {
        var granted = RolePermissions.Grants(Roles.Receptionist, "offices:delete");

        granted.Should().BeFalse();
    }

    [Theory]
    [InlineData(Roles.Doctor)]
    [InlineData(Roles.Patient)]
    [InlineData("Admin")]
    [InlineData("")]
    [InlineData("receptionist")]
    [InlineData("RECEPTIONIST")]
    public void Grants_RoleWithoutOfficeRights_ReturnsFalseForEveryPermission(string role)
    {
        var granted = OfficePermissions.Where(permission => RolePermissions.Grants(role, permission));

        granted.Should().BeEmpty();
    }

    [Fact]
    public void ForRole_Receptionist_ReturnsAllOfficePermissions()
    {
        var permissions = RolePermissions.ForRole(Roles.Receptionist);

        permissions.Should().BeEquivalentTo(OfficePermissions);
    }

    [Theory]
    [InlineData(Roles.Doctor)]
    [InlineData(Roles.Patient)]
    [InlineData("Admin")]
    public void ForRole_RoleWithoutOfficeRights_ReturnsEmptySet(string role)
    {
        var permissions = RolePermissions.ForRole(role);

        permissions.Should().BeEmpty();
    }
}
