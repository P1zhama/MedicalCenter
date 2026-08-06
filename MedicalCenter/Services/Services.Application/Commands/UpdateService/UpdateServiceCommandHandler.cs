using Common.Abstractions.Eventing;
using Common.Abstractions.Security;
using Common.Domain;
using ErrorOr;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Services.Application.Common.Interfaces;
using Services.Domain.ValueObjects;

namespace Services.Application.Commands.UpdateService;

public sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ErrorOr<Success>>
{
    private readonly IServiceCommandRepository _serviceRepository;
    private readonly IServiceCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;

    public UpdateServiceCommandHandler(
        IServiceCommandRepository serviceRepository,
        IServiceCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (service is null)
            return Error.NotFound("Service.NotFound", "Service was not found.");

        if (!await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
            return Error.Validation("Service.CategoryId", "Please, choose the service category");

        var name = TextNormalization.CollapseWhitespace(request.Name);

        if (await _serviceRepository.ExistsWithNameAsync(name, service.SpecializationId, request.Id, cancellationToken))
            return Error.Conflict("Service.DuplicateName", "Service with this name already exists in the specialization.");

        var now = _timeProvider.GetUtcNow();
        var updatedBy = _currentUserProvider.User?.Id ?? Guid.Empty;
        var expectedVersion = service.Version;

        var deactivated = service.Update(
            name,
            Price.Create(request.Price),
            request.CategoryId,
            request.Status,
            updatedBy,
            now);

        _serviceRepository.Update(service, expectedVersion);

        if (deactivated)
        {
            await _eventPublisher.PublishAsync(
                new ServiceDeactivatedEvent(service.Id, service.SpecializationId, now.UtcDateTime),
                cancellationToken);
        }

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Service.ConcurrencyConflict", "Service was modified by another operation. Please retry.");

        return Result.Success;
    }
}
