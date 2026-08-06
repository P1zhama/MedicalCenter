using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Services.Application.Common.Dtos;
using Services.Domain.Constants;

namespace Services.Application.Queries.GetSpecializationById;

public record GetSpecializationByIdQuery(Guid Id) : IRequest<ErrorOr<SpecializationDto>>, IAuthorizedRequest
{
    public string RequiredPermission => Permissions.ViewSpecializations;
}
