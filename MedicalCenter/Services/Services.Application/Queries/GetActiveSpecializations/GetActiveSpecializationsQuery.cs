using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;

namespace Services.Application.Queries.GetActiveSpecializations;

public record GetActiveSpecializationsQuery() : IRequest<ErrorOr<IReadOnlyList<PublicSpecializationDto>>>;
