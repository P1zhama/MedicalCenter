using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Api.Protos;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly ServicesService.ServicesServiceClient _servicesClient;

    public ServicesController(ServicesService.ServicesServiceClient servicesClient)
    {
        _servicesClient = servicesClient;
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var response = await _servicesClient.GetServiceCatalogAsync(new GetServiceCatalogRequest());

        var categories = response.Categories
            .Select(category => new CatalogCategoryWebResponse(
                category.CategoryId,
                category.Name,
                category.Specializations
                    .Select(specialization => new CatalogSpecializationWebResponse(
                        specialization.SpecializationId,
                        specialization.Name,
                        specialization.Services
                            .Select(service => new CatalogServiceWebResponse(
                                service.ServiceId,
                                service.Name,
                                ParsePrice(service.Price)))
                            .ToList()))
                    .ToList()))
            .ToList();

        return Ok(new ServiceCatalogWebResponse(categories));
    }

    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations()
    {
        var response = await _servicesClient.GetSpecializationsAsync(new GetSpecializationsRequest());

        var specializations = response.Specializations
            .Select(specialization => new SpecializationListItemWebResponse(
                specialization.SpecializationId,
                specialization.Name,
                specialization.Status))
            .ToList();

        return Ok(specializations);
    }

    [HttpGet("specializations/active")]
    public async Task<IActionResult> GetActiveSpecializations()
    {
        var response = await _servicesClient.GetActiveSpecializationsAsync(new GetActiveSpecializationsRequest());

        var specializations = response.Specializations
            .Select(specialization => new PublicSpecializationWebResponse(
                specialization.SpecializationId,
                specialization.Name))
            .ToList();

        return Ok(specializations);
    }

    [HttpGet("specializations/{id}")]
    public async Task<IActionResult> GetSpecializationById(string id)
    {
        var response = await _servicesClient.GetSpecializationByIdAsync(
            new GetSpecializationByIdRequest { SpecializationId = id });

        return Ok(new SpecializationWebResponse(
            response.SpecializationId,
            response.Name,
            response.Status,
            response.Services.Select(ToServiceListItem).ToList()));
    }

    [HttpPost("specializations")]
    public async Task<IActionResult> CreateSpecialization([FromBody] CreateSpecializationWebRequest request)
    {
        var grpcRequest = new CreateSpecializationRequest
        {
            Name = request.Name,
            Status = request.Status ?? string.Empty
        };

        foreach (var service in request.Services ?? [])
        {
            grpcRequest.Services.Add(new NewServiceItem
            {
                Name = service.Name,
                Price = FormatPrice(service.Price),
                CategoryId = service.CategoryId,
                Status = service.Status ?? string.Empty
            });
        }

        var response = await _servicesClient.CreateSpecializationAsync(grpcRequest);

        return Ok(new CreatedSpecializationWebResponse(response.SpecializationId));
    }

    [HttpPut("specializations/{id}")]
    public async Task<IActionResult> UpdateSpecialization(string id, [FromBody] UpdateSpecializationWebRequest request)
    {
        await _servicesClient.UpdateSpecializationAsync(new UpdateSpecializationRequest
        {
            SpecializationId = id,
            Name = request.Name,
            Status = request.Status
        });

        return Ok();
    }

    [HttpPatch("specializations/{id}/status")]
    public async Task<IActionResult> ChangeSpecializationStatus(string id, [FromBody] ChangeStatusWebRequest request)
    {
        await _servicesClient.ChangeSpecializationStatusAsync(new ChangeSpecializationStatusRequest
        {
            SpecializationId = id,
            Status = request.Status
        });

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceById(string id)
    {
        var response = await _servicesClient.GetServiceByIdAsync(new GetServiceByIdRequest { ServiceId = id });

        return Ok(new ServiceWebResponse(
            response.ServiceId,
            response.Name,
            ParsePrice(response.Price),
            response.Status,
            response.CategoryId,
            response.CategoryName,
            response.SpecializationId,
            response.SpecializationName));
    }

    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceWebRequest request)
    {
        var response = await _servicesClient.CreateServiceAsync(new CreateServiceRequest
        {
            Name = request.Name,
            Price = FormatPrice(request.Price),
            SpecializationId = request.SpecializationId,
            CategoryId = request.CategoryId,
            Status = request.Status ?? string.Empty
        });

        return Ok(new CreatedServiceWebResponse(response.ServiceId));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(string id, [FromBody] UpdateServiceWebRequest request)
    {
        await _servicesClient.UpdateServiceAsync(new UpdateServiceRequest
        {
            ServiceId = id,
            Name = request.Name,
            Price = FormatPrice(request.Price),
            CategoryId = request.CategoryId,
            Status = request.Status
        });

        return Ok();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeServiceStatus(string id, [FromBody] ChangeStatusWebRequest request)
    {
        await _servicesClient.ChangeServiceStatusAsync(new ChangeServiceStatusRequest
        {
            ServiceId = id,
            Status = request.Status
        });

        return Ok();
    }

    private static ServiceListItemWebResponse ToServiceListItem(ServiceListItem service)
        => new(
            service.ServiceId,
            service.Name,
            ParsePrice(service.Price),
            service.Status,
            service.CategoryId,
            service.CategoryName);

    private static decimal ParsePrice(string value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ? price : 0m;

    private static string FormatPrice(decimal price)
        => price.ToString("0.##", CultureInfo.InvariantCulture);
}
