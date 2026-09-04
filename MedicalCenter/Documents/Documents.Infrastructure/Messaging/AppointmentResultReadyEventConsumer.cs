using Common.Abstractions.Eventing;
using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using MassTransit;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Documents.Infrastructure.Messaging;

public sealed class AppointmentResultReadyEventConsumer : IConsumer<AppointmentResultReadyEvent>
{
    private readonly IPdfRenderer _renderer;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AppointmentResultReadyEventConsumer> _logger;

    public AppointmentResultReadyEventConsumer(
        IPdfRenderer renderer,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<AppointmentResultReadyEventConsumer> logger)
    {
        _renderer = renderer;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentResultReadyEvent> context)
    {
        var message = context.Message;

        var result = new AppointmentResultDocumentDto(
            message.AppointmentId,
            message.PatientProfileId,
            DateOnly.FromDateTime(message.AppointmentStart),
            TimeOnly.FromDateTime(message.AppointmentStart),
            TimeOnly.FromDateTime(message.AppointmentEnd),
            message.PatientFullName,
            DateOnly.FromDateTime(message.PatientDateOfBirth),
            message.DoctorFullName,
            message.SpecializationName,
            message.ServiceName,
            message.Complaints,
            message.Conclusion,
            message.Diagnosis,
            message.Recommendations);

        var content = _renderer.RenderAppointmentResult(result);

        await _eventPublisher.PublishAsync(
            new AppointmentResultEmailRequested(
                message.PatientProfileId,
                $"appointment-result-{result.Date:yyyy-MM-dd}.pdf",
                content),
            context.CancellationToken);

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Rendered the result of appointment {AppointmentId} and queued it for delivery to patient {PatientId}",
            message.AppointmentId,
            message.PatientProfileId);
    }
}
