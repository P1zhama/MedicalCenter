using Authorization.Application.Common.Interfaces;
using Authorization.Application.Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.EventConsumers;

public class EmailDeliveryConsumer : IConsumer<EmailDeliveryRequested>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailDeliveryConsumer> _logger;

    public EmailDeliveryConsumer(
        IEmailService emailService, 
        ILogger<EmailDeliveryConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailDeliveryRequested> context)
    {
        var message = context.Message;

        _logger.LogInformation("Отправка письма для {Email}", message.ToEmail);

        await _emailService.SendEmailAsync(message.ToEmail, message.Subject, message.Body, context.CancellationToken);
    }
}
