using EmailService.Domain.Entities;

namespace EmailService.Application.Commands.SendEmail
{
    public sealed class SendEmailCommand
    {
        public EmailMessage Message { get; }
        public SendEmailCommand(EmailMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
