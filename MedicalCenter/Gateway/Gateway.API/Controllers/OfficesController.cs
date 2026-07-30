using Gateway.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Offices.Api.Protos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/offices")]
[Authorize]
public class OfficesController : ControllerBase
{
    private readonly OfficesService.OfficesServiceClient _officesClient;

    public OfficesController(OfficesService.OfficesServiceClient officesClient)
    {
        _officesClient = officesClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetOffices()
    {
        var response = await _officesClient.GetOfficesAsync(new GetOfficesRequest());

        var offices = new List<OfficeListItemWebResponse>();
        foreach (var o in response.Offices)
        {
            offices.Add(new OfficeListItemWebResponse(o.OfficeId, o.Address, o.Status, o.RegistryPhoneNumber));
        }

        return Ok(offices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOfficeById(string id)
    {
        var o = await _officesClient.GetOfficeByIdAsync(new GetOfficeByIdRequest { OfficeId = id });

        return Ok(new OfficeWebResponse(
            o.OfficeId, o.PhotoUrl, o.Address, o.City, o.Street,
            o.HouseNumber, o.OfficeNumber, o.Status, o.RegistryPhoneNumber));
    }

    [HttpPost]
    public async Task<IActionResult> CreateOffice([FromBody] CreateOfficeWebRequest request)
    {
        var response = await _officesClient.CreateOfficeAsync(new CreateOfficeRequest
        {
            City = request.City,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            OfficeNumber = request.OfficeNumber ?? string.Empty,
            RegistryPhoneNumber = request.RegistryPhoneNumber,
            PhotoUrl = request.PhotoUrl ?? string.Empty,
            Status = request.Status ?? string.Empty
        });

        return Ok(new CreatedOfficeWebResponse(response.OfficeId));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOffice(string id, [FromBody] UpdateOfficeWebRequest request)
    {
        await _officesClient.UpdateOfficeAsync(new UpdateOfficeRequest
        {
            OfficeId = id,
            City = request.City,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            OfficeNumber = request.OfficeNumber ?? string.Empty,
            RegistryPhoneNumber = request.RegistryPhoneNumber,
            PhotoUrl = request.PhotoUrl ?? string.Empty,
            Status = request.Status
        });

        return Ok();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeOfficeStatus(string id, [FromBody] ChangeOfficeStatusWebRequest request)
    {
        await _officesClient.ChangeOfficeStatusAsync(new ChangeOfficeStatusRequest
        {
            OfficeId = id,
            Status = request.Status
        });

        return Ok();
    }
}
