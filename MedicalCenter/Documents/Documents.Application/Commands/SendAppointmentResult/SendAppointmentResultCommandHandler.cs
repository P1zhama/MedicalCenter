using Common.Abstractions.Eventing;
using Common.Abstractions.Security;
using Documents.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Documents.Application.Commands.SendAppointmentResult;

public sealed class SendAppointmentResultCommandHandler
    : IRequestHandler<SendAppointmentResultCommand, ErrorOr<Success>>
{
    private readonly IAppointmentResultClient _client;
    private readonly IPdfRenderer _renderer;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<SendAppointmentResultCommandHandler> _logger;

    public SendAppointmentResultCommandHandler(
        IAppointmentResultClient client,
        IPdfRenderer renderer,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        ILogger<SendAppointmentResultCommandHandler> logger)
    {
        _client = client;
        _renderer = renderer;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(
        SendAppointmentResultCommand request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.User;

        if (user is null || !user.IsAuthenticated)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var result = await _client.GetAsync(request.AppointmentId, cancellationToken);

        if (result.IsError)
            return result.Errors;

        if (!user.HasProfile || user.ProfileId != result.Value.PatientId)
            return Error.Forbidden("Auth.Forbidden", "You are not allowed to perform this action.");

        if (!result.Value.HasResult)
            return Error.NotFound("AppointmentResult.NotFound", "Appointment result was not found.");

        var content = _renderer.RenderAppointmentResult(result.Value);

        await _eventPublisher.PublishAsync(
            new AppointmentResultEmailRequested(
                result.Value.PatientId,
                $"appointment-result-{result.Value.Date:yyyy-MM-dd}.pdf",
                content),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Appointment result {AppointmentId} was queued for delivery to patient {PatientId}",
            request.AppointmentId,
            result.Value.PatientId);

        return Result.Success;
    }
}
