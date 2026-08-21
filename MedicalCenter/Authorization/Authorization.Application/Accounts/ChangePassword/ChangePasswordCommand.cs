using ErrorOr;
using MediatR;

namespace Authorization.Application.Accounts.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : IRequest<ErrorOr<ChangePasswordResult>>;
