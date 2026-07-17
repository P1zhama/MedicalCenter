using MediatR;
using Offices.Application.Common.Dtos;

namespace Offices.Application.Queries.GetOffices;

public record GetOfficesQuery() : IRequest<IReadOnlyList<OfficeListItemDto>>;
