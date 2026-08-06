using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.Queries.GetReceptionistById;

public sealed class GetReceptionistByIdQueryHandler
    : IRequestHandler<GetReceptionistByIdQuery, ErrorOr<ReceptionistDto>>
{
    private readonly IReceptionistQueryRepository _repository;

    public GetReceptionistByIdQueryHandler(IReceptionistQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<ReceptionistDto>> Handle(
        GetReceptionistByIdQuery request,
        CancellationToken cancellationToken)
    {
        var receptionist = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (receptionist is null)
            return Error.NotFound("Receptionist.NotFound", "Receptionist was not found.");

        return receptionist;
    }
}
