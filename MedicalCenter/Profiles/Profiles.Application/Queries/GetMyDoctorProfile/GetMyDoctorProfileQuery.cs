using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetMyDoctorProfile;

public record GetMyDoctorProfileQuery() : IRequest<ErrorOr<DoctorDto>>;
