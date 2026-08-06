using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using Common.Domain;
using ErrorOr;
using MediatR;
using Services.Application.Common.Interfaces;
using Services.Domain.Models;
using Services.Domain.ValueObjects;

namespace Services.Application.Commands.CreateService;

public sealed class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ErrorOr<Guid>>
{
    private readonly IServiceCommandRepository _serviceRepository;
    private readonly ISpecializationQueryRepository _specializationQueryRepository;
    private readonly IServiceCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public CreateServiceCommandHandler(
        IServiceCommandRepository serviceRepository,
        ISpecializationQueryRepository specializationQueryRepository,
        IServiceCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _serviceRepository = serviceRepository;
        _specializationQueryRepository = specializationQueryRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        if (!await _specializationQueryRepository.IsActiveAsync(request.SpecializationId, cancellationToken))
            return Error.Validation("Service.SpecializationId", "Please, choose the specialisation");

        if (!await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
            return Error.Validation("Service.CategoryId", "Please, choose the service category");

        var name = TextNormalization.CollapseWhitespace(request.Name);

        if (await _serviceRepository.ExistsWithNameAsync(name, request.SpecializationId, null, cancellationToken))
            return Error.Conflict("Service.DuplicateName", "Service with this name already exists in the specialization.");

        var now = _timeProvider.GetUtcNow();
        var createdBy = _currentUserProvider.User?.Id ?? Guid.Empty;

        var service = Service.Create(
            _guidProvider.NewGuid(),
            name,
            Price.Create(request.Price),
            request.SpecializationId,
            request.CategoryId,
            request.Status,
            createdBy,
            now);

        await _serviceRepository.AddAsync(service, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Service.ConcurrencyConflict", "Service was modified by another operation. Please retry.");

        return service.Id;
    }
}
