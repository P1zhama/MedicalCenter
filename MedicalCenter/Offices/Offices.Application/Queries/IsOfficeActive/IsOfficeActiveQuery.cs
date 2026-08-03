using ErrorOr;
using MediatR;

namespace Offices.Application.Queries.IsOfficeActive;

public record IsOfficeActiveQuery(Guid Id) : IRequest<ErrorOr<bool>>;
