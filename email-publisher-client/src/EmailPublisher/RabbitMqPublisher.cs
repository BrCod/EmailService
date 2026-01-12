using RabbitMQ.Client;

namespace EmailPublisher
{
    internal sealed class RabbitMqPublisher : IDisposable
    {
        private readonly EmailPublisherOptions _opts;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqPublisher(EmailPublisherOptions opts)
        {
            _opts = opts;
            Connect();
        }

        private void Connect()
        {
            var factory = new ConnectionFactory
            {
                HostName = _opts.HostName,
                Port = _opts.Port,
                UserName = _opts.UserName,
                Password = _opts.Password,
                VirtualHost = _opts.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ConfirmSelect();
        }

        public async Task PublishAsync(ReadOnlyMemory<byte> payload, CancellationToken ct = default)
        {
            if (_channel is null || _channel.IsClosed)
            {
                Connect();
            }

            var props = _channel!.CreateBasicProperties();
            props.DeliveryMode = 2; // persistent
            props.ContentType = "application/json";

            _channel.BasicPublish(exchange: string.Empty, routingKey: _opts.Queue, mandatory: true, basicProperties: props, body: payload.ToArray());

            // Publisher confirms - run on thread pool to avoid blocking
            await Task.Run(() => _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5)), ct).ConfigureAwait(false);
        }

        public void Dispose()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
