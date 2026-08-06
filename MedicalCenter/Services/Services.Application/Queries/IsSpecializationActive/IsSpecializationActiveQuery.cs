using ErrorOr;
using MediatR;

namespace Services.Application.Queries.IsSpecializationActive;

public record IsSpecializationActiveQuery(Guid Id) : IRequest<ErrorOr<bool>>;
