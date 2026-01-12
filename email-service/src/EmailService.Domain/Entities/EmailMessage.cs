namespace EmailService.Domain.Entities
{
    public sealed class EmailMessage
    {
        public EmailAddress From { get; }
        public IReadOnlyList<EmailAddress> To { get; }
        public string Subject { get; }
        public string? Body { get; }
        public string? TemplateId { get; }
        public IReadOnlyDictionary<string, string> TemplateData { get; }
        public IReadOnlyList<string> Attachments { get; }
        public string TenantId { get; }
        public string CorrelationId { get; }

        public EmailMessage(
            EmailAddress from,
            IEnumerable<EmailAddress> to,
            string subject,
            string? body,
            string? templateId,
            IDictionary<string, string> templateData,
            IEnumerable<string> attachments,
            string tenantId,
            string correlationId)
        {
            From = from;
            To = to.ToList();
            Subject = subject;
            Body = body;
            TemplateId = templateId;
            TemplateData = new Dictionary<string, string>(templateData);
            Attachments = attachments.ToList();
            TenantId = tenantId;
            CorrelationId = correlationId;
        }
    }
}
