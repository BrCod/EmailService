using EmailPublisher.Models;

namespace EmailPublisher
{
    public sealed class EmailPublisher : IAsyncDisposable
    {
        private readonly EmailPublisherOptions _opts;
        private readonly RabbitMqPublisher _publisher;

        public EmailPublisher(EmailPublisherOptions options)
        {
            _opts = options;
            _publisher = new RabbitMqPublisher(options);
        }

        public async Task SendEmailAsync(EmailMessage message, CancellationToken ct = default)
        {
            var payload = CloudEventFactory.CreateEmailSendRequest(message, _opts.Source);
            await _publisher.PublishAsync(payload, ct);
        }

        public ValueTask DisposeAsync()
        {
            _publisher.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
