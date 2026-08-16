using System.Linq;
using System.Threading.Tasks;
using Appointments.Api.Protos;
using Gateway.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentsService.AppointmentsServiceClient _appointmentsClient;

    public AppointmentsController(AppointmentsService.AppointmentsServiceClient appointmentsClient)
    {
        _appointmentsClient = appointmentsClient;
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] string date,
        [FromQuery] string serviceId,
        [FromQuery] string? doctorId,
        [FromQuery] string? officeId)
    {
        var response = await _appointmentsClient.GetAvailableSlotsAsync(new GetAvailableSlotsRequest
        {
            Date = date ?? string.Empty,
            ServiceId = serviceId ?? string.Empty,
            DoctorId = doctorId ?? string.Empty,
            OfficeId = officeId ?? string.Empty
        });

        var slots = response.Slots
            .Select(slot => new AvailableSlotWebResponse(slot.StartTime, slot.DoctorIds.ToList()))
            .ToList();

        return Ok(slots);
    }
}
