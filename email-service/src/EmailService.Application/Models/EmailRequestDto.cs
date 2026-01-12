using EmailService.Domain.Entities;

namespace EmailService.Application.Models
{
    public sealed class EmailRequestDto
    {
        public string? TenantId { get; init; }
        public string? CorrelationId { get; init; }
        public EmailAddress From { get; init; } = new("no-reply@example.com");
        public List<EmailAddress> To { get; init; } = new();
        public string Subject { get; init; } = string.Empty;
        public string? Body { get; init; }
        public string? TemplateId { get; init; }
        public Dictionary<string, string>? TemplateData { get; init; }
        public List<string>? Attachments { get; init; }

        public EmailMessage ToDomain()
        {
            return new EmailMessage(
                From,
                To,
                Subject,
                Body,
                TemplateId,
                TemplateData ?? new Dictionary<string, string>(),
                Attachments ?? new List<string>(),
                TenantId ?? string.Empty,
                CorrelationId ?? Guid.NewGuid().ToString());
        }
    }
}
