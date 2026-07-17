using Gateway.API.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Offices.API.Protos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/offices")]
[Authorize(Roles = "Receptionist")]
public class OfficesController : ControllerBase
{
    private readonly OfficesService.OfficesServiceClient _officesClient;
    private readonly ILogger<OfficesController> _logger;

    public OfficesController(OfficesService.OfficesServiceClient officesClient, ILogger<OfficesController> logger)
    {
        _officesClient = officesClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetOffices()
    {
        try
        {
            var response = await _officesClient.GetOfficesAsync(new GetOfficesRequest());

            var offices = new List<OfficeListItemWebResponse>();
            foreach (var o in response.Offices)
            {
                offices.Add(new OfficeListItemWebResponse(o.OfficeId, o.Address, o.Status, o.RegistryPhoneNumber));
            }

            return Ok(offices);
        }
        catch (RpcException ex)
        {
            return MapRpcError(ex, "Error getting offices");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOfficeById(string id)
    {
        try
        {
            var o = await _officesClient.GetOfficeByIdAsync(new GetOfficeByIdRequest { OfficeId = id });

            return Ok(new OfficeWebResponse(
                o.OfficeId, o.PhotoUrl, o.Address, o.City, o.Street,
                o.HouseNumber, o.OfficeNumber, o.Status, o.RegistryPhoneNumber));
        }
        catch (RpcException ex)
        {
            return MapRpcError(ex, "Error getting office");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOffice([FromBody] CreateOfficeWebRequest request)
    {
        var grpcRequest = new CreateOfficeRequest
        {
            City = request.City,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            OfficeNumber = request.OfficeNumber ?? string.Empty,
            RegistryPhoneNumber = request.RegistryPhoneNumber,
            PhotoUrl = request.PhotoUrl ?? string.Empty,
            Status = request.Status ?? string.Empty
        };

        try
        {
            var response = await _officesClient.CreateOfficeAsync(grpcRequest);
            return Ok(new CreatedOfficeWebResponse(response.OfficeId, response.Message));
        }
        catch (RpcException ex)
        {
            return MapRpcError(ex, "Error creating office");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOffice(string id, [FromBody] UpdateOfficeWebRequest request)
    {
        var grpcRequest = new UpdateOfficeRequest
        {
            OfficeId = id,
            City = request.City,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            OfficeNumber = request.OfficeNumber ?? string.Empty,
            RegistryPhoneNumber = request.RegistryPhoneNumber,
            PhotoUrl = request.PhotoUrl ?? string.Empty,
            Status = request.Status
        };

        try
        {
            var response = await _officesClient.UpdateOfficeAsync(grpcRequest);
            return Ok(new { message = response.Message });
        }
        catch (RpcException ex)
        {
            return MapRpcError(ex, "Error updating office");
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeOfficeStatus(string id, [FromBody] ChangeOfficeStatusWebRequest request)
    {
        var grpcRequest = new ChangeOfficeStatusRequest
        {
            OfficeId = id,
            Status = request.Status
        };

        try
        {
            var response = await _officesClient.ChangeOfficeStatusAsync(grpcRequest);
            return Ok(new { message = response.Message });
        }
        catch (RpcException ex)
        {
            return MapRpcError(ex, "Error changing office status");
        }
    }

    private IActionResult MapRpcError(RpcException ex, string logMessage)
    {
        _logger.LogWarning(ex, logMessage);

        return ex.StatusCode switch
        {
            Grpc.Core.StatusCode.NotFound => NotFound(new { error = ex.Status.Detail }),
            Grpc.Core.StatusCode.InvalidArgument => BadRequest(new { error = ex.Status.Detail }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error occurred." })
        };
    }
}
