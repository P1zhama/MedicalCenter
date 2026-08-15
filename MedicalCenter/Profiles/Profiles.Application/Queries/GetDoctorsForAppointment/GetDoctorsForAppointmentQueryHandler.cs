using ErrorOr;
using MediatR;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetDoctorsForAppointment;

public sealed class GetDoctorsForAppointmentQueryHandler
    : IRequestHandler<GetDoctorsForAppointmentQuery, ErrorOr<IReadOnlyList<Guid>>>
{
    private readonly IDoctorQueryRepository _repository;

    public GetDoctorsForAppointmentQueryHandler(IDoctorQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IReadOnlyList<Guid>>> Handle(
        GetDoctorsForAppointmentQuery request,
        CancellationToken cancellationToken)
    {
        var ids = await _repository.GetAtWorkIdsAsync(request.SpecializationId, request.OfficeId, cancellationToken);

        return ErrorOrFactory.From(ids);
    }
}
