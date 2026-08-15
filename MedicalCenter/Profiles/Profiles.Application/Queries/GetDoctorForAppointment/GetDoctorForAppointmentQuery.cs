using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetDoctorForAppointment;

public record GetDoctorForAppointmentQuery(Guid Id) : IRequest<ErrorOr<DoctorForAppointmentDto>>;
