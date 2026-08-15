using ErrorOr;
using MediatR;

namespace Profiles.Application.Queries.PatientExists;

public record PatientExistsQuery(Guid Id) : IRequest<ErrorOr<bool>>;
