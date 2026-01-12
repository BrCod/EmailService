using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using EmailService.Domain.Services;
using Microsoft.Extensions.Logging;

namespace EmailService.Application.Commands.SendEmail
{
    public sealed class SendEmailCommandHandler
    {
        private readonly IEmailSender _sender;
        private readonly IEmailMessageRepository _repository;
        private readonly EmailValidationService _validator;
        private readonly ILogger<SendEmailCommandHandler> _logger;

        public SendEmailCommandHandler(
            IEmailSender sender,
            IEmailMessageRepository repository,
            EmailValidationService validator,
            ILogger<SendEmailCommandHandler> logger)
        {
            _sender = sender;
            _repository = repository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<SendEmailResult> HandleAsync(SendEmailCommand command, CancellationToken ct = default)
        {
            _validator.Validate(command.Message);

            var result = await _sender.SendAsync(command.Message, ct).ConfigureAwait(false);
            await _repository.SaveAsync(command.Message, result, ct).ConfigureAwait(false);

            _logger.LogInformation("Email send result: {Status} CorrelationId={CorrelationId}", result.Status, command.Message.CorrelationId);

            return result;
        }
    }

    public sealed class SendEmailResult
    {
        public bool Success => Status == "Sent";
        public string Status { get; init; } = "Unknown";
        public string? ProviderMessageId { get; init; }
        public string? Error { get; init; }
    }
}
