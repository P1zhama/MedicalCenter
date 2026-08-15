using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctorForAppointment;

public sealed class GetDoctorForAppointmentQueryHandler
    : IRequestHandler<GetDoctorForAppointmentQuery, ErrorOr<DoctorForAppointmentDto>>
{
    private readonly IDoctorQueryRepository _repository;

    public GetDoctorForAppointmentQueryHandler(IDoctorQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<DoctorForAppointmentDto>> Handle(
        GetDoctorForAppointmentQuery request,
        CancellationToken cancellationToken)
    {
        var doctor = await _repository.GetForAppointmentAsync(request.Id, cancellationToken);
        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        return doctor;
    }
}
