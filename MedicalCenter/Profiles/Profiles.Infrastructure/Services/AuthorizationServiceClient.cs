using Authorization.Api.Protos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Infrastructure.Services;

public class AuthorizationServiceClient : IAuthorizationServiceClient
{
    private readonly AuthInternalService.AuthInternalServiceClient _client;

    public AuthorizationServiceClient(AuthInternalService.AuthInternalServiceClient client)
    {
        _client = client;
    }

    public async Task<Guid> CreateWorkerAccountAsync(string email, string roleName, Guid createdBy, CancellationToken cancellationToken)
    {
        var request = new CreateWorkerRequest
        {
            Email = email,
            RoleName = roleName,
            CreatedBy = createdBy.ToString()
        };

        var response = await _client.CreateWorkerAccountAsync(request, cancellationToken: cancellationToken);

        return Guid.Parse(response.AccountId);
    }

    public async Task DeleteWorkerAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var request = new DeleteWorkerRequest { AccountId = accountId.ToString() };

        await _client.DeleteWorkerAccountAsync(request, cancellationToken: cancellationToken);
    }

    public async Task DeletePatientAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var request = new DeletePatientRequest { AccountId = accountId.ToString() };

        await _client.DeletePatientAccountAsync(request, cancellationToken: cancellationToken);
    }
}
