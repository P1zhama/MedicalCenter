using Authorization.Application.Common.Configurations;
using Authorization.Application.Common.Messages;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.SignUp;

public record SignUpEvent(string Email, string ConfirmationToken) : INotification;

public class SignUpEventNotificationHandler : INotificationHandler<SignUpEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EmailSettings _emailSettings;

    public SignUpEventNotificationHandler(
        IPublishEndpoint publishEndpoint, 
        IOptions<EmailSettings> emailSettings)
    {
        _publishEndpoint = publishEndpoint;
        _emailSettings = emailSettings.Value;
    }

    public async Task Handle(SignUpEvent notification, CancellationToken cancellationToken)
    {
        var confirmationLink = $"{_emailSettings.ClientAppBaseUrl}/confirm-email?token={notification.ConfirmationToken}";

        var subject = "Confirmation of registration at the Medical Center";
        var body = $@"
            <h3>Welcome to the Medical Center!</h3>
            <p>To complete the registration and proceed to creating a profile, confirm your email using the link below:</p>
            <p><a href='{confirmationLink}'>Confirm email</a></p>
            <p>If the link doesn't open, copy it to the browser: {confirmationLink}</p>";

        await _publishEndpoint.Publish(new EmailDeliveryRequested(notification.Email, subject, body), cancellationToken);
    }
}
