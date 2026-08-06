using Authorization.Api.ErrorMapping;
using Authorization.Api.Protos;
using Authorization.Application.Accounts.ConfirmEmail;
using Authorization.Application.Accounts.Refresh;
using Authorization.Application.Accounts.SignIn;
using Authorization.Application.Accounts.SignOut;
using Authorization.Application.Accounts.SignUp;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace Authorization.Api.Services;

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

    public override async Task<SignInResponse> SignIn(SignInRequest request, ServerCallContext context)
    {
        var command = new SignInCommand(request.Email, request.Password);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new SignInResponse
        {
            AccountId = result.Value.AccountId.ToString(),
            AccessToken = result.Value.AccessToken,
            AccessTokenExpiresAt = Timestamp.FromDateTimeOffset(result.Value.AccessTokenExpiresAt),
            RefreshToken = result.Value.RefreshToken,
            RefreshTokenExpiresAt = Timestamp.FromDateTimeOffset(result.Value.RefreshTokenExpiresAt)
        };
    }

    public override async Task<ConfirmEmailResponse> ConfirmEmail(ConfirmEmailRequest request, ServerCallContext context)
    {
        var command = new ConfirmEmailCommand(request.Token);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new ConfirmEmailResponse { AccountId = result.Value.ToString() };
    }

    public override async Task<RefreshResponse> Refresh(RefreshRequest request, ServerCallContext context)
    {
        var command = new RefreshCommand(request.RefreshToken);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new RefreshResponse
        {
            AccountId = result.Value.AccountId.ToString(),
            AccessToken = result.Value.AccessToken,
            AccessTokenExpiresAt = Timestamp.FromDateTimeOffset(result.Value.AccessTokenExpiresAt),
            RefreshToken = result.Value.RefreshToken,
            RefreshTokenExpiresAt = Timestamp.FromDateTimeOffset(result.Value.RefreshTokenExpiresAt)
        };
    }

    public override async Task<SignOutResponse> SignOut(SignOutRequest request, ServerCallContext context)
    {
        var command = new SignOutCommand(request.RefreshToken);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new SignOutResponse();
    }
}
