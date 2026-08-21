using System.Globalization;
using Appointments.Api.ErrorMapping;
using Appointments.Api.Protos;
using Appointments.Application.Commands.ApproveAppointment;
using Appointments.Application.Commands.CancelAppointment;
using Appointments.Application.Commands.CreateAppointment;
using Appointments.Application.Commands.CreateAppointmentByReceptionist;
using Appointments.Application.Commands.CreateAppointmentResult;
using Appointments.Application.Commands.UpdateAppointmentResult;
using Appointments.Application.Commands.RescheduleAppointment;
using Appointments.Application.Common.Dtos;
using Appointments.Application.Queries.GetAppointmentResult;
using Appointments.Application.Queries.GetAppointments;
using Appointments.Application.Queries.GetAvailableSlots;
using Appointments.Application.Queries.GetDoctorSchedule;
using Appointments.Application.Queries.GetMyAppointments;
using Appointments.Application.Queries.GetPatientAppointments;
using Appointments.Application.Queries.HasApprovedAppointment;
using Grpc.Core;
using ErrorOr;
using MediatR;

namespace Appointments.Api.Services;

public class AppointmentsGrpcService : AppointmentsService.AppointmentsServiceBase
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm";

    private readonly ISender _sender;

    public AppointmentsGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<GetAvailableSlotsResponse> GetAvailableSlots(
        GetAvailableSlotsRequest request,
        ServerCallContext context)
    {
        var query = new GetAvailableSlotsQuery(
            ParseDate(request.Date),
            ParseGuid(request.ServiceId, "service id"),
            ParseNullableGuid(request.DoctorId, "doctor id"),
            ParseNullableGuid(request.OfficeId, "office id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetAvailableSlotsResponse();

        foreach (var slot in result.Value)
        {
            var item = new AvailableSlot { StartTime = slot.StartTime.ToString(TimeFormat, CultureInfo.InvariantCulture) };

            foreach (var doctorId in slot.DoctorIds)
            {
                item.DoctorIds.Add(doctorId.ToString());
            }

            response.Slots.Add(item);
        }

        return response;
    }

    public override async Task<CreateAppointmentResponse> CreateAppointment(
        CreateAppointmentRequest request,
        ServerCallContext context)
    {
        var command = new CreateAppointmentCommand(
            ParseGuid(request.ServiceId, "service id"),
            ParseGuid(request.DoctorId, "doctor id"),
            ParseGuid(request.OfficeId, "office id"),
            ParseDate(request.Date),
            ParseTime(request.StartTime));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateAppointmentResponse { AppointmentId = result.Value.ToString() };
    }

    public override async Task<CreateAppointmentResponse> CreateAppointmentByReceptionist(
        CreateAppointmentByReceptionistRequest request,
        ServerCallContext context)
    {
        var command = new CreateAppointmentByReceptionistCommand(
            ParseGuid(request.PatientId, "patient id"),
            ParseGuid(request.ServiceId, "service id"),
            ParseGuid(request.DoctorId, "doctor id"),
            ParseGuid(request.OfficeId, "office id"),
            ParseDate(request.Date),
            ParseTime(request.StartTime));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateAppointmentResponse { AppointmentId = result.Value.ToString() };
    }

    public override async Task<RescheduleAppointmentResponse> RescheduleAppointment(
        RescheduleAppointmentRequest request,
        ServerCallContext context)
    {
        var command = new RescheduleAppointmentCommand(
            ParseGuid(request.AppointmentId, "appointment id"),
            ParseGuid(request.DoctorId, "doctor id"),
            ParseDate(request.Date),
            ParseTime(request.StartTime));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new RescheduleAppointmentResponse();
    }

    public override async Task<RescheduleAppointmentResponse> RescheduleMyAppointment(
        RescheduleAppointmentRequest request,
        ServerCallContext context)
    {
        var command = new RescheduleMyAppointmentCommand(
            ParseGuid(request.AppointmentId, "appointment id"),
            ParseGuid(request.DoctorId, "doctor id"),
            ParseDate(request.Date),
            ParseTime(request.StartTime));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new RescheduleAppointmentResponse();
    }

    public override async Task<ApproveAppointmentResponse> ApproveAppointment(
        ApproveAppointmentRequest request,
        ServerCallContext context)
    {
        var command = new ApproveAppointmentCommand(ParseGuid(request.AppointmentId, "appointment id"));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new ApproveAppointmentResponse();
    }

    public override async Task<CancelAppointmentResponse> CancelAppointment(
        CancelAppointmentRequest request,
        ServerCallContext context)
    {
        var command = new CancelAppointmentCommand(ParseGuid(request.AppointmentId, "appointment id"));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CancelAppointmentResponse();
    }

    public override async Task<AppointmentListResponse> GetAppointments(
        GetAppointmentsRequest request,
        ServerCallContext context)
    {
        var query = new GetAppointmentsQuery(
            ParseDate(request.Date),
            ParseNullableGuid(request.DoctorId, "doctor id"),
            ParseNullableGuid(request.ServiceId, "service id"),
            ParseNullableGuid(request.OfficeId, "office id"),
            NullIfEmpty(request.Status));

        return ToListResponse(await _sender.Send(query, context.CancellationToken));
    }

    public override async Task<AppointmentListResponse> GetDoctorSchedule(
        GetDoctorScheduleRequest request,
        ServerCallContext context)
    {
        var query = new GetDoctorScheduleQuery(ParseDate(request.Date));

        return ToListResponse(await _sender.Send(query, context.CancellationToken));
    }

    public override async Task<AppointmentListResponse> GetMyAppointments(
        GetMyAppointmentsRequest request,
        ServerCallContext context)
        => ToListResponse(await _sender.Send(new GetMyAppointmentsQuery(), context.CancellationToken));

    public override async Task<AppointmentListResponse> GetPatientAppointments(
        GetPatientAppointmentsRequest request,
        ServerCallContext context)
    {
        var query = new GetPatientAppointmentsQuery(ParseGuid(request.PatientId, "patient id"));

        return ToListResponse(await _sender.Send(query, context.CancellationToken));
    }

    public override async Task<CreateAppointmentResultResponse> CreateAppointmentResult(
        SaveAppointmentResultRequest request,
        ServerCallContext context)
    {
        var command = new CreateAppointmentResultCommand(
            ParseGuid(request.AppointmentId, "appointment id"),
            request.Complaints,
            request.Conclusion,
            request.Recommendations,
            NullIfEmpty(request.Diagnosis));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateAppointmentResultResponse { ResultId = result.Value.ToString() };
    }

    public override async Task<UpdateAppointmentResultResponse> UpdateAppointmentResult(
        SaveAppointmentResultRequest request,
        ServerCallContext context)
    {
        var command = new UpdateAppointmentResultCommand(
            ParseGuid(request.AppointmentId, "appointment id"),
            request.Complaints,
            request.Conclusion,
            request.Recommendations,
            NullIfEmpty(request.Diagnosis));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateAppointmentResultResponse();
    }

    public override async Task<HasApprovedAppointmentResponse> HasApprovedAppointment(
        HasApprovedAppointmentRequest request,
        ServerCallContext context)
    {
        var query = new HasApprovedAppointmentQuery(
            ParseGuid(request.DoctorId, "doctor id"),
            ParseGuid(request.PatientId, "patient id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new HasApprovedAppointmentResponse { HasApproved = result.Value };
    }

    public override async Task<AppointmentResultResponse> GetAppointmentResult(
        GetAppointmentResultRequest request,
        ServerCallContext context)
    {
        var query = new GetAppointmentResultQuery(ParseGuid(request.AppointmentId, "appointment id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var value = result.Value;

        return new AppointmentResultResponse
        {
            ResultId = value.Id?.ToString() ?? string.Empty,
            AppointmentId = value.AppointmentId.ToString(),
            Date = value.Date.ToString(DateFormat, CultureInfo.InvariantCulture),
            StartTime = value.StartTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
            EndTime = value.EndTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
            PatientId = value.PatientId.ToString(),
            PatientFirstName = value.PatientFirstName,
            PatientLastName = value.PatientLastName,
            PatientMiddleName = value.PatientMiddleName ?? string.Empty,
            PatientDateOfBirth = value.PatientDateOfBirth.ToString(DateFormat, CultureInfo.InvariantCulture),
            DoctorId = value.DoctorId.ToString(),
            DoctorFirstName = value.DoctorFirstName,
            DoctorLastName = value.DoctorLastName,
            DoctorMiddleName = value.DoctorMiddleName ?? string.Empty,
            SpecializationName = value.SpecializationName,
            ServiceId = value.ServiceId.ToString(),
            ServiceName = value.ServiceName,
            Complaints = value.Complaints ?? string.Empty,
            Conclusion = value.Conclusion ?? string.Empty,
            Recommendations = value.Recommendations ?? string.Empty,
            Diagnosis = value.Diagnosis ?? string.Empty
        };
    }

    private static AppointmentListResponse ToListResponse(ErrorOr<IReadOnlyList<AppointmentListItemDto>> result)
    {
        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new AppointmentListResponse();

        foreach (var item in result.Value)
        {
            response.Appointments.Add(new AppointmentListItem
            {
                AppointmentId = item.Id.ToString(),
                Date = item.Date.ToString(DateFormat, CultureInfo.InvariantCulture),
                StartTime = item.StartTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
                EndTime = item.EndTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
                DoctorId = item.DoctorId.ToString(),
                DoctorFirstName = item.DoctorFirstName,
                DoctorLastName = item.DoctorLastName,
                DoctorMiddleName = item.DoctorMiddleName ?? string.Empty,
                PatientId = item.PatientId.ToString(),
                PatientFirstName = item.PatientFirstName ?? string.Empty,
                PatientLastName = item.PatientLastName ?? string.Empty,
                PatientMiddleName = item.PatientMiddleName ?? string.Empty,
                PatientPhoneNumber = item.PatientPhoneNumber ?? string.Empty,
                ServiceId = item.ServiceId.ToString(),
                ServiceName = item.ServiceName,
                Status = item.Status
            });
        }

        return response;
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

    private static TimeOnly ParseTime(string value)
        => TimeOnly.TryParseExact(value, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
            ? time
            : throw new RpcException(new Status(StatusCode.InvalidArgument, "Please, select the time slot"));

    private static Guid ParseGuid(string value, string fieldName)
        => Guid.TryParse(value, out var id)
            ? id
            : throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid {fieldName} format."));

    private static Guid? ParseNullableGuid(string value, string fieldName)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return ParseGuid(value, fieldName);
    }

    private static DateOnly ParseDate(string value)
        => DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new RpcException(new Status(StatusCode.InvalidArgument, "Please, select the date"));
}
