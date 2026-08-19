using System.Collections.Generic;
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

    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] string date,
        [FromQuery] string? doctorId,
        [FromQuery] string? serviceId,
        [FromQuery] string? officeId,
        [FromQuery] string? status)
    {
        var response = await _appointmentsClient.GetAppointmentsAsync(new GetAppointmentsRequest
        {
            Date = date ?? string.Empty,
            DoctorId = doctorId ?? string.Empty,
            ServiceId = serviceId ?? string.Empty,
            OfficeId = officeId ?? string.Empty,
            Status = status ?? string.Empty
        });

        return Ok(ToWebResponse(response));
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetDoctorSchedule([FromQuery] string date)
    {
        var response = await _appointmentsClient.GetDoctorScheduleAsync(
            new GetDoctorScheduleRequest { Date = date ?? string.Empty });

        return Ok(ToWebResponse(response));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var response = await _appointmentsClient.GetMyAppointmentsAsync(new GetMyAppointmentsRequest());

        return Ok(ToWebResponse(response));
    }

    [HttpGet("patients/{patientId}")]
    public async Task<IActionResult> GetPatientAppointments(string patientId)
    {
        var response = await _appointmentsClient.GetPatientAppointmentsAsync(
            new GetPatientAppointmentsRequest { PatientId = patientId });

        return Ok(ToWebResponse(response));
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

    private static List<AppointmentListItemWebResponse> ToWebResponse(AppointmentListResponse response)
        => response.Appointments
            .Select(item => new AppointmentListItemWebResponse(
                item.AppointmentId,
                item.Date,
                item.StartTime,
                item.EndTime,
                item.DoctorId,
                item.DoctorFirstName,
                item.DoctorLastName,
                item.DoctorMiddleName,
                item.PatientId,
                item.PatientFirstName,
                item.PatientLastName,
                item.PatientMiddleName,
                item.PatientPhoneNumber,
                item.ServiceId,
                item.ServiceName,
                item.Status))
            .ToList();
}
