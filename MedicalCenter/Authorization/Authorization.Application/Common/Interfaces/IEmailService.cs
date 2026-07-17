using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);   
}