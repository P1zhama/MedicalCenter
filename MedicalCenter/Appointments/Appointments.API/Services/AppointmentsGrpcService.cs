using System.Globalization;
using Appointments.Api.ErrorMapping;
using Appointments.Api.Protos;
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
