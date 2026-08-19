using System.Globalization;
using Appointments.Api.ErrorMapping;
using Appointments.Api.Protos;
using Appointments.Application.Commands.ApproveAppointment;
using Appointments.Application.Commands.CancelAppointment;
using Appointments.Application.Commands.CreateAppointment;
using Appointments.Application.Commands.CreateAppointmentByReceptionist;
using Appointments.Application.Commands.RescheduleAppointment;
using Appointments.Application.Queries.GetAvailableSlots;
using Grpc.Core;
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
