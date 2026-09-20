using Grpc.Core;
using Offices.Api.Protos;
using Offices.IntegrationTests.Support;

namespace Offices.IntegrationTests.Api;

[Collection(nameof(OfficesApiCollection))]
public sealed class OfficeDeactivationMessagingTests : IAsyncLifetime
{
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly OfficesEnvironment _environment;

    public OfficeDeactivationMessagingTests(OfficesEnvironment environment)
    {
        _environment = environment;
    }

    public ValueTask InitializeAsync() => new(_environment.ClearApiDataAsync(TestContext.Current.CancellationToken));

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ChangeOfficeStatus_ActiveOfficeDeactivated_DeliversEventToBroker()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var listener = await OfficeDeactivatedListener.StartAsync(_environment, cancellationToken);
        var headers = new Metadata { { "Authorization", $"Bearer {TestTokens.Receptionist()}" } };

        var created = await _environment.Client.CreateOfficeAsync(
            new CreateOfficeRequest
            {
                City = "Москва",
                Street = "Тверская",
                HouseNumber = "12",
                OfficeNumber = "101",
                RegistryPhoneNumber = "+74951234567",
                PhotoUrl = string.Empty,
                Status = "Active"
            },
            headers,
            cancellationToken: cancellationToken);

        await _environment.Client.ChangeOfficeStatusAsync(
            new ChangeOfficeStatusRequest { OfficeId = created.OfficeId, Status = "Inactive" },
            headers,
            cancellationToken: cancellationToken);

        var published = await listener.WaitForEventAsync(DeliveryTimeout, cancellationToken);
        published.OfficeId.Should().Be(Guid.Parse(created.OfficeId));
        published.DeactivatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
