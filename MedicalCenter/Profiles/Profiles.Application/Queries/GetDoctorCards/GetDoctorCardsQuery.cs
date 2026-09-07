using Common.Abstractions.Paging;
using ErrorOr;
using MediatR;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Queries.GetDoctorCards;

public record GetDoctorCardsQuery(string? Search, Guid? SpecializationId, Guid? OfficeId, int Page, int PageSize)
    : IRequest<ErrorOr<PagedResult<DoctorCardDto>>>;
