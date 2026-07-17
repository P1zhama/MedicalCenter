using Authorization.API.Protos;
using Authorization.Application.Commands.CreateWorkerAccount;
using Authorization.Application.Commands.DeleteWorkerAccount;
using Grpc.Core;
using MediatR;
using System;
using System.Threading.Tasks;

namespace Authorization.API.Services;

public class AuthInternalGrpcService : AuthInternalService.AuthInternalServiceBase
{
    private readonly ISender _sender;

    public AuthInternalGrpcService(ISender mediator)
    {
        _sender = mediator;
    }

    public override async Task<CreateWorkerResponse> CreateWorkerAccount(CreateWorkerRequest request, ServerCallContext context)
    {
        var command = new CreateWorkerAccountCommand(request.Email, request.RoleName, request.CreatedBy);
        var accountId = await _sender.Send(command);

        return new CreateWorkerResponse
        {
            AccountId = accountId,
            Message = "Worker account created successfully."
        };
    }

    public override async Task<DeleteWorkerResponse> DeleteWorkerAccount(DeleteWorkerRequest request, ServerCallContext context)
    {
        var success = await _sender.Send(new DeleteWorkerAccountCommand(Guid.Parse(request.AccountId)), context.CancellationToken);

        return new DeleteWorkerResponse
        {
            Success = success,
            Message = success ? "Worker account deleted successfully." : "Worker account was not found."
        };
    }
}