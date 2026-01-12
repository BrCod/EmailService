using EmailService.Domain.Entities;
using EmailService.Domain.Exceptions;

namespace EmailService.Domain.Services
{
    public sealed class EmailValidationService
    {
        public void Validate(EmailMessage message)
        {
            if (!EmailAddress.IsValid(message.From.Value))
                throw new InvalidEmailException($"Invalid 'From' address: {message.From}");

            if (message.To is null || message.To.Count == 0)
                throw new InvalidEmailException("At least one recipient is required");

            foreach (var to in message.To)
            {
                if (!EmailAddress.IsValid(to.Value))
                    throw new InvalidEmailException($"Invalid 'To' address: {to}");
            }

            if (string.IsNullOrWhiteSpace(message.Subject))
                throw new InvalidEmailException("Subject cannot be empty");

            if (string.IsNullOrWhiteSpace(message.Body) && string.IsNullOrWhiteSpace(message.TemplateId))
                throw new InvalidEmailException("Either Body or TemplateId must be provided");
        }
    }
}
