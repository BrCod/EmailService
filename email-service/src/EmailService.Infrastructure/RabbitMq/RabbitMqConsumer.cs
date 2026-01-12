using System.Text;
using System.Text.Json;
using EmailService.Application.Commands.SendEmail;
using EmailService.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EmailService.Infrastructure.Config;

namespace EmailService.Infrastructure.RabbitMq
{
    public sealed class RabbitMqConsumer : BackgroundService
    {
        private readonly RabbitMqConnectionFactory _factory;
        private readonly RabbitMqOptions _opts;
        private readonly SendEmailCommandHandler _handler;
        private readonly ILogger<RabbitMqConsumer> _logger;

        public RabbitMqConsumer(
            RabbitMqConnectionFactory factory,
            IOptions<RabbitMqOptions> options,
            SendEmailCommandHandler handler,
            ILogger<RabbitMqConsumer> logger)
        {
            _factory = factory;
            _opts = options.Value;
            _handler = handler;
            _logger = logger;
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = _factory.CreateChannel();

            // Ensure queue exists (durable, non-exclusive)
            channel.QueueDeclare(_opts.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (ch, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    var type = root.GetProperty("type").GetString();
                    if (type != "email.send.request")
                    {
                        _logger.LogWarning("Unexpected CloudEvent type: {Type}", type);
                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }
                    var dataJson = root.GetProperty("data").GetRawText();
                    var emailMessage = JsonSerializer.Deserialize<EmailMessage>(dataJson);
                    if (emailMessage is null)
                    {
                        throw new InvalidOperationException("Invalid email payload");
                    }

                    var cmd = new SendEmailCommand(emailMessage);
                    var result = await _handler.HandleAsync(cmd, stoppingToken);

                    if (result.Success)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message processing failed");
                    channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };

            channel.BasicConsume(queue: _opts.Queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
    }
}
