using Authorization.API.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Gateway.API.Models;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthClientController : ControllerBase
{
    private readonly AuthService.AuthServiceClient _authClient;
    private readonly ILogger<AuthClientController> _logger;

    public AuthClientController(AuthService.AuthServiceClient authClient, ILogger<AuthClientController> logger)
    {
        _authClient = authClient ?? throw new ArgumentNullException(nameof(authClient));
        _logger = logger;
    }

    [HttpPost("sign-up")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUp([FromBody] SignUpWebRequest request)
    {
        try
        {
            var grpcRequest = new SignUpRequest
            {
                Email = request.Email,
                Password = request.Password
            };

            var grpcResponse = await _authClient.SignUpAsync(grpcRequest);

            return Ok(new { message = grpcResponse.Message });
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error during gRPC SignUp call");

            if (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument ||
                ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
            {
                return BadRequest(new { error = ex.Status.Detail });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error occurred." });
        }
    }

    [HttpPost("sign-in")]
    [ProducesResponseType(typeof(AuthWebResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] SignInWebRequest request)
    {
        try
        {
            var grpcRequest = new SignInRequest
            {
                Email = request.Email,
                Password = request.Password
            };

            var grpcResponse = await _authClient.SignInAsync(grpcRequest);

            return Ok(new AuthWebResponse(
                grpcResponse.AccessToken,
                grpcResponse.RefreshToken,
                grpcResponse.Message
            ));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed login attempt for user: {Email}", request.Email);

            if (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
            {
                return Unauthorized(new { error = ex.Status.Detail });
            }

            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("update-refresh-token")]
    [ProducesResponseType(typeof(AuthWebResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRefreshToken([FromBody] UpdateRefreshTokenWebRequest request)
    {
        try
        {
            var grpcRequest = new UpdateRefreshTokenRequest
            {
                RefreshToken = request.RefreshToken
            };

            var grpcResponse = await _authClient.UpdateRefreshTokenAsync(grpcRequest);

            return Ok(new AuthWebResponse(
                grpcResponse.AccessToken,
                grpcResponse.RefreshToken,
                grpcResponse.Message
            ));
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error during gRPC RefreshToken call");
            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("sign-out")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignOut([FromBody] SignOutWebRequest request)
    {
        try
        {
            var grpcRequest = new SignOutRequest
            {
                RefreshToken = request.RefreshToken
            };

            var grpcResponse = await _authClient.SignOutAsync(grpcRequest);

            return Ok(new { message = grpcResponse.Message });
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error during gRPC SignOut call");
            return BadRequest(new { error = ex.Status.Detail });
        }
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(AuthWebResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailWebRequest request)
    {
        try
        {
            var grpcRequest = new ConfirmEmailRequest
            {
                ConfirmToken = request.Token
            };

            var grpcResponse = await _authClient.ConfirmEmailAsync(grpcRequest);

            return Ok(new AuthWebResponse(
                grpcResponse.AccessToken,
                grpcResponse.RefreshToken,
                grpcResponse.Message
            ));
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error during gRPC ConfirmEmail call");

            if (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return BadRequest(new { error = ex.Status.Detail });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error occurred." });
        }
    }
}