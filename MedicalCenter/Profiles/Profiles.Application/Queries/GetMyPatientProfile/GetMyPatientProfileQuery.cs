using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetMyPatientProfile;

public record GetMyPatientProfileQuery() : IRequest<ErrorOr<PatientDto>>;
