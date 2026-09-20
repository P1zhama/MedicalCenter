using Grpc.Core;
using Offices.Api.Protos;
using Offices.IntegrationTests.Support;

namespace Offices.IntegrationTests.Api;

[Collection(nameof(OfficesApiCollection))]
public sealed class OfficesApiTests : IAsyncLifetime
{
    private const string PhotoUrl = "/api/documents/7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e03";

    private static readonly Guid ReferencedPhotoId = new("7c1d2e3f-4a5b-4c6d-8e9f-0a1b2c3d4e03");
    private static readonly Guid OrphanedPhotoId = new("5f6a7b8c-9d0e-4f1a-9b2c-4d5e6f7a8b31");

    private readonly OfficesEnvironment _environment;

    public OfficesApiTests(OfficesEnvironment environment)
    {
        _environment = environment;
    }

    public ValueTask InitializeAsync() => new(_environment.ClearApiDataAsync(TestContext.Current.CancellationToken));

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static Metadata Bearer(string token) => new() { { "Authorization", $"Bearer {token}" } };

    private static CreateOfficeRequest NewOfficeRequest(
        string city = "Москва",
        string street = "Тверская",
        string houseNumber = "12",
        string officeNumber = "101",
        string photoUrl = "",
        string status = "Active") => new()
        {
            City = city,
            Street = street,
            HouseNumber = houseNumber,
            OfficeNumber = officeNumber,
            RegistryPhoneNumber = "+74951234567",
            PhotoUrl = photoUrl,
            Status = status
        };

    private async Task<string> CreateOffice(CreateOfficeRequest request, CancellationToken cancellationToken)
    {
        var response = await _environment.Client.CreateOfficeAsync(
            request,
            Bearer(TestTokens.Receptionist()),
            cancellationToken: cancellationToken);

        return response.OfficeId;
    }

    private Task<OfficeResponse> ReadOffice(string officeId, CancellationToken cancellationToken)
        => _environment.Client.GetOfficeByIdAsync(
            new GetOfficeByIdRequest { OfficeId = officeId },
            Bearer(TestTokens.Receptionist()),
            cancellationToken: cancellationToken).ResponseAsync;

    [Fact]
    public async Task CreateOffice_ReceptionistToken_StoresOfficeReadableById()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var officeId = await CreateOffice(NewOfficeRequest(photoUrl: PhotoUrl), cancellationToken);

        var office = await ReadOffice(officeId, cancellationToken);
        office.Should().Be(new OfficeResponse
        {
            OfficeId = officeId,
            PhotoUrl = PhotoUrl,
            Address = "Москва, Тверская, 12, 101",
            City = "Москва",
            Street = "Тверская",
            HouseNumber = "12",
            OfficeNumber = "101",
            Status = "Active",
            RegistryPhoneNumber = "+74951234567"
        });
    }

    [Fact]
    public async Task CreateOffice_WithoutToken_ThrowsUnauthenticated()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var act = async () => await _environment.Client.CreateOfficeAsync(
            NewOfficeRequest(),
            cancellationToken: cancellationToken);

        (await act.Should().ThrowAsync<RpcException>()).Which.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task CreateOffice_DoctorToken_ThrowsPermissionDenied()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var act = async () => await _environment.Client.CreateOfficeAsync(
            NewOfficeRequest(),
            Bearer(TestTokens.Doctor()),
            cancellationToken: cancellationToken);

        (await act.Should().ThrowAsync<RpcException>()).Which.StatusCode.Should().Be(StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task CreateOffice_ExpiredToken_ThrowsUnauthenticatedWithExpiredCode()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var act = async () => await _environment.Client.CreateOfficeAsync(
            NewOfficeRequest(),
            Bearer(TestTokens.Expired()),
            cancellationToken: cancellationToken);

        var exception = (await act.Should().ThrowAsync<RpcException>()).Which;
        exception.StatusCode.Should().Be(StatusCode.Unauthenticated);
        exception.Status.Detail.Should().Be("Auth.TokenExpired");
    }

    [Fact]
    public async Task CreateOffice_MalformedToken_ThrowsUnauthenticatedWithInvalidCode()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var act = async () => await _environment.Client.CreateOfficeAsync(
            NewOfficeRequest(),
            Bearer("not-a-token"),
            cancellationToken: cancellationToken);

        var exception = (await act.Should().ThrowAsync<RpcException>()).Which;
        exception.StatusCode.Should().Be(StatusCode.Unauthenticated);
        exception.Status.Detail.Should().Be("Auth.TokenInvalid");
    }

    [Fact]
    public async Task CreateOffice_BlankCity_ThrowsInvalidArgumentWithValidatorMessage()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var act = async () => await _environment.Client.CreateOfficeAsync(
            NewOfficeRequest(city: string.Empty),
            Bearer(TestTokens.Receptionist()),
            cancellationToken: cancellationToken);

        var exception = (await act.Should().ThrowAsync<RpcException>()).Which;
        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Status.Detail.Should().Contain("Please, enter the office's city");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Closed")]
    [InlineData("1")]
    public async Task CreateOffice_UnknownStatus_ThrowsInvalidArgument(string status)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var act = async () => await _environment.Client.CreateOfficeAsync(
            NewOfficeRequest(status: status),
            Bearer(TestTokens.Receptionist()),
            cancellationToken: cancellationToken);

        var exception = (await act.Should().ThrowAsync<RpcException>()).Which;
        exception.StatusCode.Should().Be(StatusCode.InvalidArgument);
        exception.Status.Detail.Should().Be("Invalid office status.");
    }

    [Fact]
    public async Task ChangeOfficeStatus_StatusInLowerCase_DeactivatesOffice()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var officeId = await CreateOffice(NewOfficeRequest(), cancellationToken);

        await _environment.Client.ChangeOfficeStatusAsync(
            new ChangeOfficeStatusRequest { OfficeId = officeId, Status = "inactive" },
            Bearer(TestTokens.Receptionist()),
            cancellationToken: cancellationToken);

        var office = await ReadOffice(officeId, cancellationToken);
        office.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task GetOfficeById_UnknownId_ThrowsNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var act = async () => await ReadOffice(Guid.NewGuid().ToString(), cancellationToken);

        (await act.Should().ThrowAsync<RpcException>()).Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task GetActiveOffices_WithoutToken_ReturnsOnlyActiveOffices()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var activeId = await CreateOffice(NewOfficeRequest(), cancellationToken);
        await CreateOffice(
            NewOfficeRequest(city: "Казань", street: "Баумана", houseNumber: "5", officeNumber: string.Empty, status: "Inactive"),
            cancellationToken);

        var response = await _environment.Client.GetActiveOfficesAsync(
            new GetActiveOfficesRequest(),
            cancellationToken: cancellationToken);

        response.Offices.Should().ContainSingle()
            .Which.Should().Be(new PublicOffice
            {
                OfficeId = activeId,
                Address = "Москва, Тверская, 12, 101",
                PhotoUrl = string.Empty,
                RegistryPhoneNumber = "+74951234567"
            });
    }

    [Theory]
    [InlineData("Active", true)]
    [InlineData("Inactive", false)]
    public async Task IsOfficeActive_WithoutToken_ReflectsOfficeStatus(string status, bool expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var officeId = await CreateOffice(NewOfficeRequest(status: status), cancellationToken);

        var response = await _environment.Client.IsOfficeActiveAsync(
            new IsOfficeActiveRequest { OfficeId = officeId },
            cancellationToken: cancellationToken);

        response.IsActive.Should().Be(expected);
    }

    [Fact]
    public async Task GetReferencedPhotos_WithoutToken_ReturnsOnlyReferencedIds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await CreateOffice(NewOfficeRequest(photoUrl: PhotoUrl), cancellationToken);

        var request = new GetReferencedPhotosRequest
        {
            DocumentIds = { ReferencedPhotoId.ToString(), OrphanedPhotoId.ToString() }
        };
        var response = await _environment.Client.GetReferencedPhotosAsync(request, cancellationToken: cancellationToken);

        response.ReferencedIds.Should().Equal(ReferencedPhotoId.ToString());
    }
}
