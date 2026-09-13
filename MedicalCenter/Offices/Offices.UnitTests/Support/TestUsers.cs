using Common.Abstractions.Security;
using Offices.Domain.Constants;

namespace Offices.UnitTests.Support;

public static class TestUsers
{
    public static readonly Guid ReceptionistId = new("9a8b7c6d-5e4f-4a3b-8c2d-1e0f9a8b7c04");

    public static CurrentUser Receptionist { get; } = new(ReceptionistId, null, [Roles.Receptionist], []);

    public static CurrentUser Doctor { get; } =
        new(new Guid("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a05"), null, [Roles.Doctor], []);

    public static CurrentUser WithPermissions(params string[] permissions)
        => new(new Guid("2a3b4c5d-6e7f-4a8b-9c0d-1e2f3a4b5c06"), null, [], permissions);
}
