namespace EmailPublisher.Models
{
    public sealed class EmailMessage
    {
        public EmailAddress From { get; init; }
        public List<EmailAddress> To { get; init; } = new();
        public string Subject { get; init; } = string.Empty;
        public string? Body { get; init; }
        public string? TemplateId { get; init; }
        public Dictionary<string, string> TemplateData { get; init; } = new();
        public List<string> Attachments { get; init; } = new();
        public string TenantId { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    }
}
