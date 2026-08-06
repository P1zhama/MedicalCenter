using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using Common.Domain;
using ErrorOr;
using MediatR;
using Services.Application.Common.Interfaces;
using Services.Domain.Models;
using Services.Domain.ValueObjects;

namespace Services.Application.Commands.CreateSpecialization;

public sealed class CreateSpecializationCommandHandler
    : IRequestHandler<CreateSpecializationCommand, ErrorOr<Guid>>
{
    private readonly ISpecializationCommandRepository _specializationRepository;
    private readonly IServiceCommandRepository _serviceRepository;
    private readonly IServiceCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;

    public CreateSpecializationCommandHandler(
        ISpecializationCommandRepository specializationRepository,
        IServiceCommandRepository serviceRepository,
        IServiceCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider)
    {
        _specializationRepository = specializationRepository;
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateSpecializationCommand request, CancellationToken cancellationToken)
    {
        var name = TextNormalization.CollapseWhitespace(request.Name);

        if (await _specializationRepository.ExistsWithNameAsync(name, null, cancellationToken))
            return Error.Conflict("Specialization.DuplicateName", "Specialization with this name already exists.");

        foreach (var categoryId in request.Services.Select(service => service.CategoryId).Distinct())
        {
            if (!await _categoryRepository.ExistsAsync(categoryId, cancellationToken))
                return Error.Validation("Service.CategoryId", "Please, choose the service category");
        }

        var now = _timeProvider.GetUtcNow();
        var createdBy = _currentUserProvider.User?.Id ?? Guid.Empty;

        var specialization = Specialization.Create(
            _guidProvider.NewGuid(),
            name,
            request.Status,
            createdBy,
            now);

        await _specializationRepository.AddAsync(specialization, cancellationToken);

        foreach (var item in request.Services)
        {
            var service = Service.Create(
                _guidProvider.NewGuid(),
                item.Name,
                Price.Create(item.Price),
                specialization.Id,
                item.CategoryId,
                item.Status,
                createdBy,
                now);

            await _serviceRepository.AddAsync(service, cancellationToken);
        }

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Specialization.ConcurrencyConflict", "Specialization was modified by another operation. Please retry.");

        return specialization.Id;
    }
}
