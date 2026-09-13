using Common.Abstractions.Security;
using Offices.Application.Commands.ChangeOfficeStatus;
using Offices.Application.Commands.CreateOffice;
using Offices.Application.Commands.UpdateOffice;
using Offices.Application.Queries.GetActiveOffices;
using Offices.Application.Queries.GetOfficeById;
using Offices.Application.Queries.GetOffices;
using Offices.Application.Queries.GetReferencedPhotos;
using Offices.Application.Queries.IsOfficeActive;
using Offices.Domain.Constants;
using Offices.Domain.Enums;

namespace Offices.UnitTests.Application.Authorization;

public sealed class RequestPermissionsTests
{
    private static readonly Guid OfficeId = new("4d5e6f7a-8b9c-4d0e-9f1a-2b3c4d5e6f20");

    public static TheoryData<IAuthorizedRequest, string> ProtectedRequests => new()
    {
        {
            new CreateOfficeCommand("Москва", "Тверская", "12", null, "+74951234567", null, OfficeStatus.Active),
            Permissions.CreateOffice
        },
        {
            new UpdateOfficeCommand(OfficeId, "Москва", "Тверская", "12", null, "+74951234567", null, OfficeStatus.Active),
            Permissions.EditOffice
        },
        { new ChangeOfficeStatusCommand(OfficeId, OfficeStatus.Inactive), Permissions.ChangeOfficeStatus },
        { new GetOfficesQuery(), Permissions.ViewOffices },
        { new GetOfficeByIdQuery(OfficeId), Permissions.ViewOffices }
    };

    public static TheoryData<object> OpenRequests => new()
    {
        new GetActiveOfficesQuery(),
        new IsOfficeActiveQuery(OfficeId),
        new GetReferencedPhotosQuery([OfficeId])
    };

    [Theory]
    [MemberData(nameof(ProtectedRequests))]
    public void RequiredPermission_ProtectedRequest_MatchesItsOperation(IAuthorizedRequest request, string expectedPermission)
    {
        request.RequiredPermission.Should().Be(expectedPermission);
    }

    [Theory]
    [MemberData(nameof(OpenRequests))]
    public void OpenRequest_DoesNotRequirePermission(object request)
    {
        request.Should().NotBeAssignableTo<IAuthorizedRequest>();
    }
}
