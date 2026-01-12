using System.Reflection;
using EmailService.Infrastructure.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EmailService.Infrastructure.RabbitMq
{
    public sealed class RabbitMqConnectionFactory : IDisposable
    {
        private readonly RabbitMqOptions _opts;
        private IConnection? _connection;
        private readonly object _lock = new();

        public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options)
        {
            _opts = options.Value;
        }

        public IConnection GetConnection()
        {
            if (_connection is { IsOpen: true }) return _connection;
            lock (_lock)
            {
                if (_connection is { IsOpen: true }) return _connection;
                var factory = new ConnectionFactory
                {
                    HostName = _opts.HostName,
                    Port = _opts.Port,
                    UserName = _opts.UserName,
                    Password = _opts.Password,
                    VirtualHost = _opts.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                };
                _connection = factory.CreateConnection();
                return _connection;
            }
        }

        public IModel CreateChannel()
        {
            var conn = GetConnection();
            var channel = conn.CreateModel();
            channel.BasicQos(0, _opts.PrefetchCount, false);
            
            // Enable async event dispatching for AsyncEventingBasicConsumer
            var property = channel.GetType().GetProperty("DispatchConsumersAsync");
            if (property?.CanWrite == true)
            {
                property.SetValue(channel, true);
            }
            
            return channel;
        }

        public void Dispose()
        {
            try { _connection?.Dispose(); } catch { }
        }
    }
}
