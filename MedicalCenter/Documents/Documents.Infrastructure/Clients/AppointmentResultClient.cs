using Appointments.Api.Protos;
using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using ErrorOr;
using Grpc.Core;

namespace Documents.Infrastructure.Clients;

public sealed class AppointmentResultClient : IAppointmentResultClient
{
    private readonly AppointmentsService.AppointmentsServiceClient _client;

    public AppointmentResultClient(AppointmentsService.AppointmentsServiceClient client)
    {
        _client = client;
    }

    public async Task<ErrorOr<AppointmentResultDocumentDto>> GetAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetAppointmentResultAsync(
                new GetAppointmentResultRequest { AppointmentId = appointmentId.ToString() },
                cancellationToken: cancellationToken);

            return Map(response);
        }
        catch (RpcException exception)
        {
            return MapError(exception);
        }
    }

    private static AppointmentResultDocumentDto Map(AppointmentResultResponse response)
        => new(
            Guid.Parse(response.AppointmentId),
            DateOnly.Parse(response.Date),
            TimeOnly.Parse(response.StartTime),
            TimeOnly.Parse(response.EndTime),
            FullName(response.PatientLastName, response.PatientFirstName, response.PatientMiddleName),
            ParseDate(response.PatientDateOfBirth),
            FullName(response.DoctorLastName, response.DoctorFirstName, response.DoctorMiddleName),
            response.SpecializationName,
            response.ServiceName,
            NullIfEmpty(response.Complaints),
            NullIfEmpty(response.Conclusion),
            NullIfEmpty(response.Diagnosis),
            NullIfEmpty(response.Recommendations));

    private static Error MapError(RpcException exception) => exception.StatusCode switch
    {
        StatusCode.NotFound => Error.NotFound("Appointment.NotFound", exception.Status.Detail),
        StatusCode.PermissionDenied => Error.Forbidden("Auth.Forbidden", exception.Status.Detail),
        StatusCode.Unauthenticated => Error.Unauthorized("Auth.Unauthenticated", exception.Status.Detail),
        StatusCode.InvalidArgument => Error.Validation("Appointment.Invalid", exception.Status.Detail),
        _ => Error.Failure("Appointment.Unavailable", "Appointment service is unavailable.")
    };

    private static string FullName(string lastName, string firstName, string middleName)
        => string.Join(' ', new[] { lastName, firstName, middleName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

    private static DateOnly ParseDate(string value)
        => DateOnly.TryParse(value, out var parsed) ? parsed : default;

    private static string? NullIfEmpty(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
