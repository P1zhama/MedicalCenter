using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Application.Queries.GetSpecializationById;

public sealed class GetSpecializationByIdQueryHandler
    : IRequestHandler<GetSpecializationByIdQuery, ErrorOr<SpecializationDto>>
{
    private readonly ISpecializationQueryRepository _repository;

    public GetSpecializationByIdQueryHandler(ISpecializationQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<SpecializationDto>> Handle(
        GetSpecializationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var specialization = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (specialization is null)
            return Error.NotFound("Specialization.NotFound", "Specialization was not found.");

        return specialization;
    }
}
