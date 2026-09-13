using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using Common.Domain;
using Microsoft.Extensions.Time.Testing;
using Offices.Application.Commands.CreateOffice;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.Domain.ValueObjects;
using Offices.UnitTests.Support;

namespace Offices.UnitTests.Application.Commands;

public sealed class CreateOfficeCommandHandlerTests
{
    private static readonly Guid NewOfficeId = new("7b8c9d0e-1f2a-4b3c-8d4e-5f6a7b8c9d14");
    private static readonly DateTimeOffset Now = new(2026, 3, 15, 10, 30, 0, TimeSpan.Zero);

    private static readonly CreateOfficeCommand Command = new(
        City: "Москва",
        Street: "Тверская",
        HouseNumber: "12",
        OfficeNumber: "101",
        RegistryPhoneNumber: "+74951234567",
        PhotoUrl: "/api/documents/9d0e1f2a-3b4c-4d5e-8f6a-7b8c9d0e1f15",
        Status: OfficeStatus.Active);

    private readonly IOfficeCommandRepository _repository = Substitute.For<IOfficeCommandRepository>();
    private readonly ICurrentUserProvider _currentUserProvider = Substitute.For<ICurrentUserProvider>();
    private readonly IGuidProvider _guidProvider = Substitute.For<IGuidProvider>();
    private readonly CreateOfficeCommandHandler _handler;
    private Office? _savedOffice;

    public CreateOfficeCommandHandlerTests()
    {
        _guidProvider.NewGuid().Returns(NewOfficeId);
        _currentUserProvider.User.Returns(TestUsers.Receptionist);
        _repository
            .AddAsync(Arg.Do<Office>(office => _savedOffice = office), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _handler = new CreateOfficeCommandHandler(
            _repository,
            _currentUserProvider,
            new FakeTimeProvider(Now),
            _guidProvider);
    }

    private Office SavedOffice
        => _savedOffice ?? throw new InvalidOperationException("The handler did not save an office.");

    [Fact]
    public async Task Handle_ValidCommand_SavesOfficeOnceAndReturnsItsId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await _handler.Handle(Command, cancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(NewOfficeId);
        SavedOffice.Id.Should().Be(NewOfficeId);
        await _repository.Received(1).AddAsync(Arg.Any<Office>(), cancellationToken);
    }

    [Theory]
    [InlineData(OfficeStatus.Active)]
    [InlineData(OfficeStatus.Inactive)]
    public async Task Handle_ValidCommand_SavesOfficeBuiltFromCommand(OfficeStatus status)
    {
        await _handler.Handle(Command with { Status = status }, TestContext.Current.CancellationToken);

        SavedOffice.Address.Should().Be(Address.Create("Москва", "Тверская", "12", "101"));
        SavedOffice.RegistryPhoneNumber.Should().Be("+74951234567");
        SavedOffice.PhotoUrl.Should().Be(Command.PhotoUrl);
        SavedOffice.Status.Should().Be(status);
        SavedOffice.Version.Should().Be(1);
        SavedOffice.Audit.Should().Be(new AuditInfo(TestUsers.ReceptionistId, Now, null, null));
    }

    [Fact]
    public async Task Handle_NoCurrentUser_RecordsEmptyCreator()
    {
        _currentUserProvider.User.Returns((CurrentUser?)null);

        await _handler.Handle(Command, TestContext.Current.CancellationToken);

        SavedOffice.Audit.CreatedBy.Should().Be(Guid.Empty);
    }
}
