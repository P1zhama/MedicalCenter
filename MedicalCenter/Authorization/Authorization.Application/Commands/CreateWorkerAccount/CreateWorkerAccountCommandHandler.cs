using Authorization.Application.Common.Interfaces;
using Authorization.Application.Events;
using Authorization.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.CreateWorkerAccount;

public class CreateWorkerAccountCommandHandler : IRequestHandler<CreateWorkerAccountCommand, string>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateWorkerAccountCommandHandler> _logger;

    public CreateWorkerAccountCommandHandler(
        IAccountRepository accountRepository, 
        IPublisher publisher,
        IRoleRepository roleRepository,
        ILogger<CreateWorkerAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _publisher = publisher;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<string> Handle(CreateWorkerAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting account creation process for worker {Email} by {Creator}", request.Email, request.CreatedBy);

        var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
        if (role == null)
        {
            _logger.LogWarning("Attempted to create worker with non-existent role: {RoleName}", request.RoleName);
            throw new ArgumentException($"Role '{request.RoleName}' does not exist.");
        }

        var isUnique = await _accountRepository.IsEmailUniqueAsync(request.Email, cancellationToken);
        if (!isUnique)
        {
            _logger.LogWarning("Worker creation failed. Email {Email} is already registered.", request.Email);
            throw new InvalidOperationException("Email is already registered.");
        }

        string generatedPassword = GenerateRandomPassword(12);
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword);

        var accountId = Guid.NewGuid();
        var newAccount = new Account
        {
            Id = accountId,
            Email = request.Email,
            Password = passwordHash,
            IsEmailVerified = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        newAccount.AccountRoles.Add(new AccountRole
        {
            AccountId = accountId,
            RoleId = role.Id
        });

        await _accountRepository.AddAsync(newAccount, cancellationToken);
        _logger.LogInformation("Worker account {AccountId} successfully saved to database.", accountId);


        await _publisher.Publish(
            new WorkerCreatedEvent(request.Email, generatedPassword, role.Name),
            cancellationToken);

        return accountId.ToString();
    }

    private string GenerateRandomPassword(int length)
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
        var chars = new char[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] randomBytes = new byte[length];
            rng.GetBytes(randomBytes);
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[randomBytes[i] % validChars.Length];
            }
        }
        return new string(chars);
    }
}