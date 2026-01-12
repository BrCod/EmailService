using EmailService.Application.Commands.SendEmail;
using EmailService.Domain.Entities;

namespace EmailService.Application.Interfaces
{
    public interface IEmailMessageRepository
    {
        Task SaveAsync(EmailMessage message, SendEmailResult result, CancellationToken ct = default);
    }
}
