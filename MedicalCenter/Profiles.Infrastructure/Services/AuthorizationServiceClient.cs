using Authorization.API.Protos;
using Profiles.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Infrastructure.Services;

public class AuthorizationServiceClient : IAuthorizationServiceClient
{
    private readonly AuthInternalService.AuthInternalServiceClient _client;

    public AuthorizationServiceClient(AuthInternalService.AuthInternalServiceClient client)
    {
        _client = client;
    }

    public async Task<Guid> CreateWorkerAccountAsync(string email, string roleName, string createdBy, CancellationToken cancellationToken)
    {
        var request = new CreateWorkerRequest
        {
            Email = email,
            RoleName = roleName,
            CreatedBy = createdBy
        };

        var response = await _client.CreateWorkerAccountAsync(request, cancellationToken: cancellationToken);

        return Guid.Parse(response.AccountId);
    }

    public async Task DeleteWorkerAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var request = new DeleteWorkerRequest { AccountId = accountId.ToString() };

        await _client.DeleteWorkerAccountAsync(request, cancellationToken: cancellationToken);
    }
}
