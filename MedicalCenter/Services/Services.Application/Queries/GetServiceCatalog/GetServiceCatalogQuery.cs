using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;

namespace Services.Application.Queries.GetServiceCatalog;

public record GetServiceCatalogQuery() : IRequest<ErrorOr<ServiceCatalogDto>>;
