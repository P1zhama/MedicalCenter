using Authorization.API.Protos;
using Authorization.Application.Accounts.SignUp;
using Grpc.Core;
using MediatR;

namespace Authorization.API.Services;

public sealed class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly ISender _sender;

    public AuthGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<SignUpResponse> SignUp(SignUpRequest request, ServerCallContext context)
    {
        var command = new SignUpCommand(request.Email, request.Password);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new SignUpResponse { AccountId = result.Value.ToString() };
    }
}
