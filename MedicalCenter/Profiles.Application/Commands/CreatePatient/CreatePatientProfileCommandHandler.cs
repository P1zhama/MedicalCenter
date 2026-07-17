using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Application.Commands;

public class CreatePatientProfileCommandHandler : IRequestHandler<CreatePatientProfileCommand, ProfileCreationResult>
{
    private readonly IPatientRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePatientProfileCommandHandler(IPatientRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ProfileCreationResult> Handle(CreatePatientProfileCommand request, CancellationToken cancellationToken)
    {
        // Ищем совпадение среди непривязанных профилей (AC-4, AC-9 из US-8).
        // Сам алгоритм весов теперь живёт в репозитории (см. PatientRepository.GetBestMatchAsync)
        var bestMatch = await _repository.GetBestMatchAsync(
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.DateOfBirth,
            cancellationToken);

        // AC-6: похожий профиль найден — не создаём и не привязываем,
        // отдаём его данные, чтобы пользователь подтвердил "это я" / "это не я"
        if (bestMatch != null)
        {
            var matchedInfo = new MatchedProfileDto(
                bestMatch.FirstName,
                bestMatch.LastName,
                bestMatch.MiddleName,
                bestMatch.DateOfBirth
            );

            return new ProfileCreationResult(true, bestMatch.Id, matchedInfo, null);
        }

        // AC-5 и AC-8: совпадений нет — создаём новый профиль и сразу привязываем к аккаунту
        var newProfile = new Patient
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(newProfile, cancellationToken);

        await _publishEndpoint.Publish(
            new ProfileLinkedToAccountEvent(request.AccountId, newProfile.Id, DateTime.UtcNow),
            cancellationToken);

        return new ProfileCreationResult(false, null, null, newProfile.Id);
    }
}
