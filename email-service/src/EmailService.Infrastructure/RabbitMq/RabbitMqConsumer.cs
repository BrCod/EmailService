using System.Text;
using System.Text.Json;
using EmailService.Application.Commands.SendEmail;
using EmailService.Application.Models;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting RabbitMQ consumer for queue: {Queue}", _opts.Queue);
            
            var messageQueueTcs = new TaskCompletionSource<bool>();
            
            try
            {
                var channel = _factory.CreateChannel();
                _logger.LogInformation("Channel created successfully");

                // Ensure queue exists (durable, non-exclusive)
                channel.QueueDeclare(_opts.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _logger.LogInformation("Queue declared successfully");

                // Set QoS explicitly
                channel.BasicQos(prefetchSize: 0, prefetchCount: _opts.PrefetchCount, global: false);
                _logger.LogInformation("QoS set to prefetch {PrefetchCount}", _opts.PrefetchCount);

                // Use AsyncEventingBasicConsumer for proper async handling
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) => await HandleMessageAsync((IModel)model, ea);

                var consumerTag = channel.BasicConsume(queue: _opts.Queue, autoAck: false, consumer: consumer);
                _logger.LogInformation("Consumer started with tag {ConsumerTag}, listening for messages", consumerTag);
                
                // Keep the consumer alive until cancellation is requested
                _ = messageQueueTcs.Task;
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RabbitMQ consumer stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start RabbitMQ consumer");
                throw;
            }
        }

        private async Task HandleMessageAsync(object model, BasicDeliverEventArgs ea)
        {
            _logger.LogInformation("Message received from queue");
            var channel = (IModel)model;
            string? json = null;
            try
            {
                json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("Message JSON: {Json}", json);
                
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var type = root.GetProperty("type").GetString();
                _logger.LogInformation("CloudEvent type: {Type}", type);
                
                if (type != "email.send.request")
                {
                    _logger.LogWarning("Unexpected CloudEvent type: {Type}", type);
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var dataJson = root.GetProperty("data").GetRawText();
                _logger.LogInformation("Deserializing email message from data: {DataJson}", dataJson);
                
                var jsonOptions = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };
                var emailDto = JsonSerializer.Deserialize<EmailRequestDto>(dataJson, jsonOptions);
                if (emailDto is null)
                {
                    throw new InvalidOperationException("Invalid email payload");
                }

                var emailMessage = emailDto.ToDomain();

                _logger.LogInformation("Sending email via handler");
                var cmd = new SendEmailCommand(emailMessage);
                var result = await _handler.HandleAsync(cmd, CancellationToken.None);

                if (result.Success)
                {
                    _logger.LogInformation("Email sent successfully");
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("Email sending failed: {Error}", result.Error);
                    channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message processing failed. JSON: {Json}", json ?? "null");
                channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        }
    }
}
