using Authorization.Api.Protos;
using Gateway.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService.AuthServiceClient _authClient;

    public AuthController(AuthService.AuthServiceClient authClient)
    {
        _authClient = authClient;
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpWebRequest request)
    {
        var grpcResponse = await _authClient.SignUpAsync(new SignUpRequest
        {
            Email = request.Email,
            Password = request.Password
        });

        return Ok(new SignUpWebResponse(grpcResponse.AccountId));
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInWebRequest request)
    {
        var grpcResponse = await _authClient.SignInAsync(new SignInRequest
        {
            Email = request.Email,
            Password = request.Password
        });

        return Ok(new SignInWebResponse(
            grpcResponse.AccountId,
            grpcResponse.AccessToken,
            grpcResponse.AccessTokenExpiresAt.ToDateTimeOffset(),
            grpcResponse.RefreshToken,
            grpcResponse.RefreshTokenExpiresAt.ToDateTimeOffset()));
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailWebRequest request)
    {
        var grpcResponse = await _authClient.ConfirmEmailAsync(new ConfirmEmailRequest
        {
            Token = request.Token
        });

        return Ok(new ConfirmEmailWebResponse(grpcResponse.AccountId));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshWebRequest request)
    {
        var grpcResponse = await _authClient.RefreshAsync(new RefreshRequest
        {
            RefreshToken = request.RefreshToken
        });

        return Ok(new RefreshWebResponse(
            grpcResponse.AccountId,
            grpcResponse.AccessToken,
            grpcResponse.AccessTokenExpiresAt.ToDateTimeOffset(),
            grpcResponse.RefreshToken,
            grpcResponse.RefreshTokenExpiresAt.ToDateTimeOffset()));
    }

    [HttpPost("sign-out")]
    public async Task<IActionResult> SignOut([FromBody] SignOutWebRequest request)
    {
        await _authClient.SignOutAsync(new SignOutRequest
        {
            RefreshToken = request.RefreshToken
        });

        return NoContent();
    }
}
