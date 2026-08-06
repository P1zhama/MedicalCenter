using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetMyReceptionistProfile;

public record GetMyReceptionistProfileQuery() : IRequest<ErrorOr<ReceptionistDto>>;
