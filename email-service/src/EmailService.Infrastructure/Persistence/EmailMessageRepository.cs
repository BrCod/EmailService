using EmailService.Application.Commands.SendEmail;
using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EmailService.Infrastructure.Persistence
{
    public sealed class EmailMessageRepository : IEmailMessageRepository
    {
        private readonly ILogger<EmailMessageRepository> _logger;
        public EmailMessageRepository(ILogger<EmailMessageRepository> logger)
        {
            _logger = logger;
        }

        public Task SaveAsync(EmailMessage message, SendEmailResult result, CancellationToken ct = default)
        {
            _logger.LogInformation("Persisting email result: Status={Status} Tenant={TenantId} Correlation={CorrelationId}", result.Status, message.TenantId, message.CorrelationId);
            // In production, insert into durable store
            return Task.CompletedTask;
        }
    }
}
