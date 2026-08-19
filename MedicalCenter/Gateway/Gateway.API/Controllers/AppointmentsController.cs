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

    [HttpPost("me")]
    public async Task<IActionResult> CreateMyAppointment([FromBody] CreateAppointmentWebRequest request)
    {
        var response = await _appointmentsClient.CreateAppointmentAsync(new CreateAppointmentRequest
        {
            ServiceId = request.ServiceId,
            DoctorId = request.DoctorId,
            OfficeId = request.OfficeId,
            Date = request.Date,
            StartTime = request.StartTime
        });

        return Ok(new CreatedAppointmentWebResponse(response.AppointmentId));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentByReceptionistWebRequest request)
    {
        var response = await _appointmentsClient.CreateAppointmentByReceptionistAsync(
            new CreateAppointmentByReceptionistRequest
            {
                PatientId = request.PatientId,
                ServiceId = request.ServiceId,
                DoctorId = request.DoctorId,
                OfficeId = request.OfficeId,
                Date = request.Date,
                StartTime = request.StartTime
            });

        return Ok(new CreatedAppointmentWebResponse(response.AppointmentId));
    }

    [HttpPut("me/{id}")]
    public async Task<IActionResult> RescheduleMyAppointment(string id, [FromBody] RescheduleAppointmentWebRequest request)
    {
        await _appointmentsClient.RescheduleMyAppointmentAsync(new RescheduleAppointmentRequest
        {
            AppointmentId = id,
            DoctorId = request.DoctorId,
            Date = request.Date,
            StartTime = request.StartTime
        });

        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> RescheduleAppointment(string id, [FromBody] RescheduleAppointmentWebRequest request)
    {
        await _appointmentsClient.RescheduleAppointmentAsync(new RescheduleAppointmentRequest
        {
            AppointmentId = id,
            DoctorId = request.DoctorId,
            Date = request.Date,
            StartTime = request.StartTime
        });

        return Ok();
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> ApproveAppointment(string id)
    {
        await _appointmentsClient.ApproveAppointmentAsync(new ApproveAppointmentRequest { AppointmentId = id });

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAppointment(string id)
    {
        await _appointmentsClient.CancelAppointmentAsync(new CancelAppointmentRequest { AppointmentId = id });

        return NoContent();
    }
}
