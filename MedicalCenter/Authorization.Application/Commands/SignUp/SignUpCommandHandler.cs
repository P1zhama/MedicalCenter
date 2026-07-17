using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.SignUp;

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, Unit>
{
    private readonly IMediator _mediator;
    private readonly IRoleRepository _roleRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public SignUpCommandHandler(
        IAccountRepository accountRepository,
        IMediator mediator,
        IRoleRepository roleRepository,
        IJwtTokenService jwtTokenService)
    {
        _accountRepository = accountRepository;
        _mediator = mediator;
        _roleRepository = roleRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Unit> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var isEmailUnique = await _accountRepository.IsEmailUniqueAsync(request.Email, cancellationToken);
        if (!isEmailUnique)
        {
            throw new InvalidOperationException("Someone already uses this email");
        }

        var patientRole = await _roleRepository.GetByNameAsync("Patient", cancellationToken);
        if (patientRole == null)
        {
            throw new InvalidOperationException("Critical error: The 'Patient' role was not found in the database.");
        }
        
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var accountId = Guid.NewGuid();
        var newAccount = new Account
        {
            Id = accountId,
            Email = request.Email,
            Password = passwordHash,
            IsEmailVerified = false,
                        
            CreatedBy = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = request.Email,
            UpdatedAt = DateTime.UtcNow
        };

        newAccount.AccountRoles.Add(new AccountRole
        {
            AccountId = accountId,
            RoleId = patientRole.Id
        });

        await _accountRepository.AddAsync(newAccount, cancellationToken);
               
        var confirmationToken = _jwtTokenService.GenerateEmailConfirmationToken(newAccount.Email);

        await _mediator.Publish(new SignUpEvent(newAccount.Email, confirmationToken), cancellationToken);

        return Unit.Value;
    }
}