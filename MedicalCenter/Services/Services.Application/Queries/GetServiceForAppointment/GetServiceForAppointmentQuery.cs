using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;

namespace Services.Application.Queries.GetServiceForAppointment;

public record GetServiceForAppointmentQuery(Guid Id) : IRequest<ErrorOr<ServiceForAppointmentDto>>;
