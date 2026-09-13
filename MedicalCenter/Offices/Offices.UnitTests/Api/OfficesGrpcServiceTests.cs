using ErrorOr;
using Grpc.Core;
using MediatR;
using Offices.Api.Protos;
using Offices.Api.Services;
using Offices.Application.Commands.ChangeOfficeStatus;
using Offices.Application.Commands.CreateOffice;
using Offices.Application.Commands.UpdateOffice;
using Offices.Application.Common.Dtos;
using Offices.Application.Queries.GetOfficeById;
using Offices.Application.Queries.GetReferencedPhotos;
using Offices.Application.Queries.IsOfficeActive;
using Offices.Domain.Enums;
using Offices.UnitTests.Support;

namespace Offices.UnitTests.Api;

public sealed class OfficesGrpcServiceTests
{
    private const string PhotoUrl = "/api/documents/7a8b9c0d-1e2f-4a3b-8c4d-5e6f7a8b9c23";

    private static readonly Guid OfficeId = new("8c9d0e1f-2a3b-4c4d-9e5f-6a7b8c9d0e24");
    private static readonly Guid SecondOfficeId = new("9d0e1f2a-3b4c-4d5e-8f6a-7b8c9d0e1f25");
    private static readonly Guid ReferencedPhotoId = new("4e5f6a7b-8c9d-4e0f-8a1b-3c4d5e6f7a30");
    private static readonly Guid OrphanedPhotoId = new("5f6a7b8c-9d0e-4f1a-9b2c-4d5e6f7a8b31");

    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly OfficesGrpcService _service;
    private readonly ServerCallContext _context;

    public OfficesGrpcServiceTests()
    {
        _service = new OfficesGrpcService(_sender);
        _context = TestServerCallContext.Create(cancellationToken: TestContext.Current.CancellationToken);
    }

    public static TheoryData<string, OfficeStatus> KnownStatuses => new()
    {
        { "Active", OfficeStatus.Active },
        { "active", OfficeStatus.Active },
        { "ACTIVE", OfficeStatus.Active },
        { "Inactive", OfficeStatus.Inactive },
        { "inactive", OfficeStatus.Inactive },
        { "INACTIVE", OfficeStatus.Inactive }
    };

    public static TheoryData<string> UnknownStatuses =>
    [
        "",
        "   ",
        "1",
        "2",
        "999",
        " Active",
        "Inactive ",
        "Closed"
    ];

    public static TheoryData<string> MalformedIds =>
    [
        "",
        "not-a-guid",
        "12345"
    ];

    private void GivenSenderReturns<TResponse>(TResponse response)
        => _sender.Send(Arg.Any<IRequest<TResponse>>(), Arg.Any<CancellationToken>()).Returns(response);

    private static CreateOfficeRequest CreateRequest(string status = "Active") => new()
    {
        City = "Москва",
        Street = "Тверская",
        HouseNumber = "12",
        OfficeNumber = "101",
        RegistryPhoneNumber = "+74951234567",
        PhotoUrl = PhotoUrl,
        Status = status
    };

    private static UpdateOfficeRequest UpdateRequest(string status = "Active", string? officeId = null) => new()
    {
        OfficeId = officeId ?? OfficeId.ToString(),
        City = "Санкт-Петербург",
        Street = "Невский проспект",
        HouseNumber = "88",
        OfficeNumber = "7",
        RegistryPhoneNumber = "+78121234567",
        PhotoUrl = PhotoUrl,
        Status = status
    };

    private static async Task<RpcException> ShouldFailWith(Func<Task> act, StatusCode expectedStatusCode)
    {
        var exception = (await act.Should().ThrowAsync<RpcException>()).Which;
        exception.StatusCode.Should().Be(expectedStatusCode);

        return exception;
    }

    [Fact]
    public async Task CreateOffice_ValidRequest_SendsCommandWithRequestValues()
    {
        GivenSenderReturns<ErrorOr<Guid>>(OfficeId);

        await _service.CreateOffice(CreateRequest(), _context);

        await _sender.Received(1).Send(
            new CreateOfficeCommand("Москва", "Тверская", "12", "101", "+74951234567", PhotoUrl, OfficeStatus.Active),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOffice_EmptyOptionalFields_SendsNulls()
    {
        GivenSenderReturns<ErrorOr<Guid>>(OfficeId);
        var request = CreateRequest();
        request.OfficeNumber = string.Empty;
        request.PhotoUrl = string.Empty;

        await _service.CreateOffice(request, _context);

        await _sender.Received(1).Send(
            Arg.Is<CreateOfficeCommand>(command => command.OfficeNumber == null && command.PhotoUrl == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOffice_HandlerSucceeded_ReturnsNewOfficeId()
    {
        GivenSenderReturns<ErrorOr<Guid>>(OfficeId);

        var response = await _service.CreateOffice(CreateRequest(), _context);

        response.OfficeId.Should().Be(OfficeId.ToString());
    }

    [Fact]
    public async Task CreateOffice_AnyRequest_ForwardsCallCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        var context = TestServerCallContext.Create(cancellationToken: cancellation.Token);
        GivenSenderReturns<ErrorOr<Guid>>(OfficeId);

        await _service.CreateOffice(CreateRequest(), context);

        await _sender.Received(1).Send(Arg.Any<CreateOfficeCommand>(), cancellation.Token);
    }

    [Theory]
    [MemberData(nameof(KnownStatuses))]
    public async Task CreateOffice_KnownStatusInAnyCase_SendsParsedStatus(string status, OfficeStatus expected)
    {
        GivenSenderReturns<ErrorOr<Guid>>(OfficeId);

        await _service.CreateOffice(CreateRequest(status), _context);

        await _sender.Received(1).Send(
            Arg.Is<CreateOfficeCommand>(command => command.Status == expected),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(UnknownStatuses))]
    public async Task CreateOffice_UnknownStatus_ThrowsInvalidArgumentWithoutSending(string status)
    {
        var exception = await ShouldFailWith(
            () => _service.CreateOffice(CreateRequest(status), _context),
            StatusCode.InvalidArgument);

        exception.Status.Detail.Should().Be("Invalid office status.");
        _sender.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOffice_HandlerReturnedError_ThrowsMappedRpcException()
    {
        GivenSenderReturns<ErrorOr<Guid>>(Error.Validation("City", "Please, enter the office's city"));

        var exception = await ShouldFailWith(
            () => _service.CreateOffice(CreateRequest(), _context),
            StatusCode.InvalidArgument);

        exception.Status.Detail.Should().Be("Please, enter the office's city");
    }

    [Fact]
    public async Task UpdateOffice_ValidRequest_SendsCommandWithRequestValues()
    {
        GivenSenderReturns<ErrorOr<Success>>(Result.Success);

        await _service.UpdateOffice(UpdateRequest(), _context);

        await _sender.Received(1).Send(
            new UpdateOfficeCommand(
                OfficeId,
                "Санкт-Петербург",
                "Невский проспект",
                "88",
                "7",
                "+78121234567",
                PhotoUrl,
                OfficeStatus.Active),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateOffice_EmptyOptionalFields_SendsNulls()
    {
        GivenSenderReturns<ErrorOr<Success>>(Result.Success);
        var request = UpdateRequest();
        request.OfficeNumber = string.Empty;
        request.PhotoUrl = string.Empty;

        await _service.UpdateOffice(request, _context);

        await _sender.Received(1).Send(
            Arg.Is<UpdateOfficeCommand>(command => command.OfficeNumber == null && command.PhotoUrl == null),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(KnownStatuses))]
    public async Task UpdateOffice_KnownStatusInAnyCase_SendsParsedStatus(string status, OfficeStatus expected)
    {
        GivenSenderReturns<ErrorOr<Success>>(Result.Success);

        await _service.UpdateOffice(UpdateRequest(status), _context);

        await _sender.Received(1).Send(
            Arg.Is<UpdateOfficeCommand>(command => command.Status == expected),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(UnknownStatuses))]
    public async Task UpdateOffice_UnknownStatus_ThrowsInvalidArgumentWithoutSending(string status)
    {
        var exception = await ShouldFailWith(
            () => _service.UpdateOffice(UpdateRequest(status), _context),
            StatusCode.InvalidArgument);

        exception.Status.Detail.Should().Be("Invalid office status.");
        _sender.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(MalformedIds))]
    public async Task UpdateOffice_MalformedOfficeId_ThrowsInvalidArgumentWithoutSending(string officeId)
    {
        var exception = await ShouldFailWith(
            () => _service.UpdateOffice(UpdateRequest(officeId: officeId), _context),
            StatusCode.InvalidArgument);

        exception.Status.Detail.Should().Be("Invalid office id format.");
        _sender.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateOffice_OfficeNotFound_ThrowsNotFound()
    {
        GivenSenderReturns<ErrorOr<Success>>(Error.NotFound("Office.NotFound", "Office was not found."));

        var exception = await ShouldFailWith(
            () => _service.UpdateOffice(UpdateRequest(), _context),
            StatusCode.NotFound);

        exception.Status.Detail.Should().Be("Office was not found.");
    }

    [Theory]
    [MemberData(nameof(KnownStatuses))]
    public async Task ChangeOfficeStatus_KnownStatusInAnyCase_SendsCommandWithParsedStatus(string status, OfficeStatus expected)
    {
        GivenSenderReturns<ErrorOr<Success>>(Result.Success);

        await _service.ChangeOfficeStatus(
            new ChangeOfficeStatusRequest { OfficeId = OfficeId.ToString(), Status = status },
            _context);

        await _sender.Received(1).Send(new ChangeOfficeStatusCommand(OfficeId, expected), Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(UnknownStatuses))]
    public async Task ChangeOfficeStatus_UnknownStatus_ThrowsInvalidArgumentWithoutSending(string status)
    {
        var exception = await ShouldFailWith(
            () => _service.ChangeOfficeStatus(
                new ChangeOfficeStatusRequest { OfficeId = OfficeId.ToString(), Status = status },
                _context),
            StatusCode.InvalidArgument);

        exception.Status.Detail.Should().Be("Invalid office status.");
        _sender.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(MalformedIds))]
    public async Task ChangeOfficeStatus_MalformedOfficeId_ThrowsInvalidArgumentWithoutSending(string officeId)
    {
        var exception = await ShouldFailWith(
            () => _service.ChangeOfficeStatus(
                new ChangeOfficeStatusRequest { OfficeId = officeId, Status = "Active" },
                _context),
            StatusCode.InvalidArgument);

        exception.Status.Detail.Should().Be("Invalid office id format.");
        _sender.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task GetOffices_OfficesExist_ReturnsEveryOffice()
    {
        IReadOnlyList<OfficeListItemDto> offices =
        [
            new(OfficeId, "Москва, Тверская, 12, 101", "Active", "+74951234567"),
            new(SecondOfficeId, "Санкт-Петербург, Невский проспект, 88", "Inactive", "+78121234567")
        ];
        GivenSenderReturns(ErrorOrFactory.From(offices));

        var response = await _service.GetOffices(new GetOfficesRequest(), _context);

        response.Offices.Should().Equal(
            new OfficeListItem
            {
                OfficeId = OfficeId.ToString(),
                Address = "Москва, Тверская, 12, 101",
                Status = "Active",
                RegistryPhoneNumber = "+74951234567"
            },
            new OfficeListItem
            {
                OfficeId = SecondOfficeId.ToString(),
                Address = "Санкт-Петербург, Невский проспект, 88",
                Status = "Inactive",
                RegistryPhoneNumber = "+78121234567"
            });
    }

    [Fact]
    public async Task GetActiveOffices_OfficesExist_ReturnsEveryOffice()
    {
        IReadOnlyList<PublicOfficeDto> offices =
        [
            new(OfficeId, "Москва, Тверская, 12, 101", PhotoUrl, "+74951234567")
        ];
        GivenSenderReturns(ErrorOrFactory.From(offices));

        var response = await _service.GetActiveOffices(new GetActiveOfficesRequest(), _context);

        response.Offices.Should().Equal(new PublicOffice
        {
            OfficeId = OfficeId.ToString(),
            Address = "Москва, Тверская, 12, 101",
            PhotoUrl = PhotoUrl,
            RegistryPhoneNumber = "+74951234567"
        });
    }

    [Fact]
    public async Task GetActiveOffices_OfficeWithoutPhoto_ReturnsEmptyPhotoUrl()
    {
        IReadOnlyList<PublicOfficeDto> offices =
        [
            new(OfficeId, "Москва, Тверская, 12, 101", null, "+74951234567")
        ];
        GivenSenderReturns(ErrorOrFactory.From(offices));

        var response = await _service.GetActiveOffices(new GetActiveOfficesRequest(), _context);

        response.Offices.Should().ContainSingle().Which.PhotoUrl.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOfficeById_ExistingOffice_SendsParsedIdAndReturnsEveryField()
    {
        GivenSenderReturns<ErrorOr<OfficeDto>>(new OfficeDto(
            OfficeId,
            PhotoUrl,
            "Москва, Тверская, 12, 101",
            "Москва",
            "Тверская",
            "12",
            "101",
            "Active",
            "+74951234567"));

        var response = await _service.GetOfficeById(new GetOfficeByIdRequest { OfficeId = OfficeId.ToString() }, _context);

        response.Should().Be(new OfficeResponse
        {
            OfficeId = OfficeId.ToString(),
            PhotoUrl = PhotoUrl,
            Address = "Москва, Тверская, 12, 101",
            City = "Москва",
            Street = "Тверская",
            HouseNumber = "12",
            OfficeNumber = "101",
            Status = "Active",
            RegistryPhoneNumber = "+74951234567"
        });
        await _sender.Received(1).Send(new GetOfficeByIdQuery(OfficeId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOfficeById_OfficeWithoutOptionalFields_ReturnsEmptyStrings()
    {
        GivenSenderReturns<ErrorOr<OfficeDto>>(new OfficeDto(
            OfficeId,
            null,
            "Санкт-Петербург, Невский проспект, 88",
            "Санкт-Петербург",
            "Невский проспект",
            "88",
            null,
            "Inactive",
            "+78121234567"));

        var response = await _service.GetOfficeById(new GetOfficeByIdRequest { OfficeId = OfficeId.ToString() }, _context);

        response.PhotoUrl.Should().BeEmpty();
        response.OfficeNumber.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOfficeById_OfficeNotFound_ThrowsNotFound()
    {
        GivenSenderReturns<ErrorOr<OfficeDto>>(Error.NotFound("Office.NotFound", "Office was not found."));

        await ShouldFailWith(
            () => _service.GetOfficeById(new GetOfficeByIdRequest { OfficeId = OfficeId.ToString() }, _context),
            StatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(MalformedIds))]
    public async Task GetOfficeById_MalformedOfficeId_ThrowsInvalidArgumentWithoutSending(string officeId)
    {
        await ShouldFailWith(
            () => _service.GetOfficeById(new GetOfficeByIdRequest { OfficeId = officeId }, _context),
            StatusCode.InvalidArgument);

        _sender.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsOfficeActive_HandlerAnswered_ReturnsAnswer(bool isActive)
    {
        GivenSenderReturns<ErrorOr<bool>>(isActive);

        var response = await _service.IsOfficeActive(new IsOfficeActiveRequest { OfficeId = OfficeId.ToString() }, _context);

        response.IsActive.Should().Be(isActive);
        await _sender.Received(1).Send(new IsOfficeActiveQuery(OfficeId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReferencedPhotos_ValidIds_SendsParsedIdsAndReturnsReferencedOnes()
    {
        GivenSenderReturns(ErrorOrFactory.From<IReadOnlyList<Guid>>([ReferencedPhotoId]));
        var request = new GetReferencedPhotosRequest
        {
            DocumentIds = { ReferencedPhotoId.ToString(), OrphanedPhotoId.ToString() }
        };

        var response = await _service.GetReferencedPhotos(request, _context);

        response.ReferencedIds.Should().Equal(ReferencedPhotoId.ToString());
        await _sender.Received(1).Send(
            Arg.Is<GetReferencedPhotosQuery>(query =>
                query.DocumentIds.SequenceEqual(new[] { ReferencedPhotoId, OrphanedPhotoId })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReferencedPhotos_MalformedId_ThrowsInvalidArgumentWithoutSending()
    {
        var request = new GetReferencedPhotosRequest { DocumentIds = { ReferencedPhotoId.ToString(), "not-a-guid" } };

        await ShouldFailWith(() => _service.GetReferencedPhotos(request, _context), StatusCode.InvalidArgument);

        _sender.ReceivedCalls().Should().BeEmpty();
    }
}
