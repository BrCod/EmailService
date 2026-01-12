using EmailService.Application.Commands.SendEmail;
using EmailService.Domain.Entities;

namespace EmailService.Application.Interfaces
{
    public interface IEmailSender
    {
        Task<SendEmailResult> SendAsync(EmailMessage message, CancellationToken ct = default);
    }
}
