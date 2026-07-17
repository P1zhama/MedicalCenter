using MediatR;
using Offices.Application.Common.Dtos;
using System;

namespace Offices.Application.Queries.GetOfficeById;

public record GetOfficeByIdQuery(Guid Id) : IRequest<OfficeDto>;
