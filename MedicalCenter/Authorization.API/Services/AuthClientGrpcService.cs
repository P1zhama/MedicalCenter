using Authorization.API.Protos;
using Authorization.Application.Commands.RefreshToken;
using Authorization.Application.Commands.SignIn;
using Authorization.Application.Commands.SignOut;
using Authorization.Application.Commands.SignUp;
using Authorization.Application.Commands.ConfirmEmail;
using Grpc.Core;
using MediatR;
using System;
using System.Threading.Tasks;

namespace Authorization.API.Services;

public class AuthClientGrpcService : AuthService.AuthServiceBase
{
    private readonly ISender _sender;

    public AuthClientGrpcService(ISender mediator)
    {
        _sender = mediator;
    }

    public override async Task<SignUpResponse> SignUp(SignUpRequest request, ServerCallContext context)
    {
        try
        {
            var command = new SignUpCommand(request.Email, request.Password);
            await _sender.Send(command);

            return new SignUpResponse
            {
                Message = "Registration successful! Please check your email."
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
    }

    public override async Task<SignInResponse> SignIn(SignInRequest request, ServerCallContext context)
    {
        try
        {
            var command = new SignInCommand(request.Email, request.Password);
            var result = await _sender.Send(command);

            return new SignInResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                Message = result.Message
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
    }

    public override async Task<SignOutResponse> SignOut(SignOutRequest request, ServerCallContext context)
    {
        var command = new SignOutCommand(request.RefreshToken);
        await _sender.Send(command);
                
        return new SignOutResponse
        {
            Message = "You have been logged out successfully."
        };
    }

    public override async Task<UpdateRefreshTokenResponse> UpdateRefreshToken(UpdateRefreshTokenRequest request, ServerCallContext context)
    {
        var command = new UpdateRefreshTokenCommand(request.RefreshToken);
        var result = await _sender.Send(command);

        return new UpdateRefreshTokenResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            Message = result.Message
        };
    }

    public override async Task<ConfirmEmailResponse> ConfirmEmail(ConfirmEmailRequest request, ServerCallContext context)
    {
        var command = new ConfirmEmailCommand(request.ConfirmToken);

        try
        {
            var result = await _sender.Send(command);

            return new ConfirmEmailResponse
            {
                ConfirmStatus = true,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                Message = result.Message
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }
}