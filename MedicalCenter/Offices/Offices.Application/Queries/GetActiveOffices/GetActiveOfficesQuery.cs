using ErrorOr;
using MediatR;
using Offices.Application.Common.Dtos;

namespace Offices.Application.Queries.GetActiveOffices;

public record GetActiveOfficesQuery() : IRequest<ErrorOr<IReadOnlyList<PublicOfficeDto>>>;
