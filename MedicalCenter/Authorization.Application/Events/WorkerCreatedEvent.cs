using MediatR;

namespace Authorization.Application.Events;

public record WorkerCreatedEvent(string Email, string GeneratedPassword, string RoleName) : INotification;
