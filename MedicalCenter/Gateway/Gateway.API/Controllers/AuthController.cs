using Authorization.API.Protos;
using Gateway.API.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService.AuthServiceClient _authClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService.AuthServiceClient authClient, ILogger<AuthController> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    [HttpPost("sign-up")]
    [ProducesResponseType(typeof(SignUpWebResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] SignUpWebRequest request)
    {
        var grpcRequest = new SignUpRequest
        {
            Email = request.Email,
            Password = request.Password
        };

        try
        {
            var grpcResponse = await _authClient.SignUpAsync(grpcRequest);

            return Ok(new SignUpWebResponse(grpcResponse.AccountId));
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC SignUp call failed");

            return ex.StatusCode switch
            {
                Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { error = ex.Status.Detail }),
                Grpc.Core.StatusCode.AlreadyExists => Conflict(new { error = ex.Status.Detail }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error occurred." })
            };
        }
    }
}
