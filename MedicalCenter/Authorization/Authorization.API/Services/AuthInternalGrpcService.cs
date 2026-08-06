using Authorization.Api.ErrorMapping;
using Authorization.Api.Protos;
using Authorization.Application.Accounts.CreateWorkerAccount;
using Authorization.Application.Accounts.DeletePatientAccount;
using Authorization.Application.Accounts.DeleteWorkerAccount;
using Grpc.Core;
using MediatR;

namespace Authorization.Api.Services;

public sealed class AuthInternalGrpcService : AuthInternalService.AuthInternalServiceBase
{
    private readonly ISender _sender;

    public AuthInternalGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CreateWorkerResponse> CreateWorkerAccount(CreateWorkerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CreatedBy, out var createdBy))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid created by id."));

        var command = new CreateWorkerAccountCommand(request.Email, request.RoleName, createdBy);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateWorkerResponse
        {
            AccountId = result.Value.ToString()
        };
    }

    public override async Task<DeleteWorkerResponse> DeleteWorkerAccount(DeleteWorkerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.AccountId, out var accountId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid account id."));

        var command = new DeleteWorkerAccountCommand(accountId);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new DeleteWorkerResponse();
    }

    public override async Task<DeletePatientResponse> DeletePatientAccount(
        DeletePatientRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.AccountId, out var accountId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid account id."));

        var command = new DeletePatientAccountCommand(accountId);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new DeletePatientResponse();
    }
}
