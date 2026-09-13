using ErrorOr;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;
using Offices.Application.Queries.GetOfficeById;

namespace Offices.UnitTests.Application.Queries;

public sealed class GetOfficeByIdQueryHandlerTests
{
    private static readonly Guid OfficeId = new("0e1f2a3b-4c5d-4e6f-8a7b-8c9d0e1f2a16");

    private readonly IOfficeQueryRepository _repository = Substitute.For<IOfficeQueryRepository>();
    private readonly GetOfficeByIdQueryHandler _handler;

    public GetOfficeByIdQueryHandlerTests()
    {
        _handler = new GetOfficeByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_OfficeExists_ReturnsIt()
    {
        var office = new OfficeDto(
            OfficeId,
            PhotoUrl: null,
            Address: "Москва, Тверская, 12, 101",
            City: "Москва",
            Street: "Тверская",
            HouseNumber: "12",
            OfficeNumber: "101",
            Status: "Active",
            RegistryPhoneNumber: "+74951234567");
        _repository.GetByIdAsync(OfficeId, Arg.Any<CancellationToken>()).Returns(office);

        var result = await _handler.Handle(new GetOfficeByIdQuery(OfficeId), TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(office);
    }

    [Fact]
    public async Task Handle_OfficeMissing_ReturnsNotFound()
    {
        _repository.GetByIdAsync(OfficeId, Arg.Any<CancellationToken>()).Returns((OfficeDto?)null);

        var result = await _handler.Handle(new GetOfficeByIdQuery(OfficeId), TestContext.Current.CancellationToken);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Office.NotFound");
    }
}
