using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetDoctorCardById;

public record GetDoctorCardByIdQuery(Guid Id) : IRequest<ErrorOr<DoctorCardDto>>;
