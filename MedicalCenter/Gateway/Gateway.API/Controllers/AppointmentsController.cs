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
    public async Task<IActionResult> GetMyAppointments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _appointmentsClient.GetMyAppointmentsAsync(new GetMyAppointmentsRequest
        {
            Page = page,
            PageSize = pageSize
        });

        return Ok(ToPagedWebResponse(response));
    }

    [HttpGet("patients/{patientId}")]
    public async Task<IActionResult> GetPatientAppointments(
        string patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _appointmentsClient.GetPatientAppointmentsAsync(new GetPatientAppointmentsRequest
        {
            PatientId = patientId,
            Page = page,
            PageSize = pageSize
        });

        return Ok(ToPagedWebResponse(response));
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

    [HttpGet("{id}/result")]
    public async Task<IActionResult> GetAppointmentResult(string id)
    {
        var r = await _appointmentsClient.GetAppointmentResultAsync(
            new GetAppointmentResultRequest { AppointmentId = id });

        return Ok(new AppointmentResultWebResponse(
            r.ResultId, r.AppointmentId, r.Date, r.StartTime, r.EndTime,
            r.PatientId, r.PatientFirstName, r.PatientLastName, r.PatientMiddleName, r.PatientDateOfBirth,
            r.DoctorId, r.DoctorFirstName, r.DoctorLastName, r.DoctorMiddleName, r.SpecializationName,
            r.ServiceId, r.ServiceName,
            r.Complaints, r.Conclusion, r.Recommendations, r.Diagnosis));
    }

    [HttpPost("{id}/result")]
    public async Task<IActionResult> CreateAppointmentResult(string id, [FromBody] SaveAppointmentResultWebRequest request)
    {
        var response = await _appointmentsClient.CreateAppointmentResultAsync(new SaveAppointmentResultRequest
        {
            AppointmentId = id,
            Complaints = request.Complaints,
            Conclusion = request.Conclusion,
            Recommendations = request.Recommendations,
            Diagnosis = request.Diagnosis ?? string.Empty
        });

        return Ok(new CreatedAppointmentResultWebResponse(response.ResultId));
    }

    [HttpPut("{id}/result")]
    public async Task<IActionResult> UpdateAppointmentResult(string id, [FromBody] SaveAppointmentResultWebRequest request)
    {
        await _appointmentsClient.UpdateAppointmentResultAsync(new SaveAppointmentResultRequest
        {
            AppointmentId = id,
            Complaints = request.Complaints,
            Conclusion = request.Conclusion,
            Recommendations = request.Recommendations,
            Diagnosis = request.Diagnosis ?? string.Empty
        });

        return Ok();
    }

    private static PagedWebResponse<AppointmentListItemWebResponse> ToPagedWebResponse(AppointmentListResponse response)
        => new(
            ToWebResponse(response),
            response.Page,
            response.PageSize,
            response.TotalCount);

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
