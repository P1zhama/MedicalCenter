using MediatR;
using System;

namespace Authorization.Application.Commands.DeleteWorkerAccount;

public record DeleteWorkerAccountCommand(Guid AccountId) : IRequest<bool>;
