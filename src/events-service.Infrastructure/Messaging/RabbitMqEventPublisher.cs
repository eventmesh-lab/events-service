using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using events_service.Domain.Events;
using events_service.Domain.Ports;

namespace events_service.Infrastructure.Messaging
{
    /// <summary>
    /// Implementación del publicador de eventos de dominio usando RabbitMQ.
    /// Publica eventos en el exchange eventos.domain.events.
    /// </summary>
    public class RabbitMqEventPublisher : IDomainEventPublisher, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _exchangeName;
        private readonly JsonSerializerOptions _jsonOptions;
        private IConnection _connection;
        private IModel _channel;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Inicializa una nueva instancia del publicador.
        /// </summary>
        /// <param name="connectionFactory">Factory para crear conexiones RabbitMQ.</param>
        /// <param name="exchangeName">Nombre del exchange donde se publican los eventos.</param>
        public RabbitMqEventPublisher(IConnectionFactory connectionFactory, string exchangeName)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));

            _exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            _connectionFactory = connectionFactory;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Obtiene la conexión, creándola si es necesario.
        /// </summary>
        private IConnection GetConnection()
        {
            if (_connection != null && _connection.IsOpen)
                return _connection;

            lock (_lockObject)
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    _connection = _connectionFactory.CreateConnection();
                }
            }

            return _connection;
        }

        /// <summary>
        /// Obtiene el canal, creándolo si es necesario.
        /// </summary>
        private IModel GetChannel()
        {
            var connection = GetConnection();

            if (_channel != null && _channel.IsOpen)
                return _channel;

            lock (_lockObject)
            {
                if (_channel == null || !_channel.IsOpen)
                {
                    _channel = connection.CreateModel();

                    // Declarar exchange
                    _channel.ExchangeDeclare(
                        exchange: _exchangeName,
                        type: ExchangeType.Topic,
                        durable: true,
                        autoDelete: false);
                }
            }

            return _channel;
        }

        /// <summary>
        /// Publica un evento de dominio en RabbitMQ.
        /// </summary>
        /// <param name="domainEvent">Evento de dominio a publicar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent == null)
                return Task.CompletedTask;

            try
            {
                var channel = GetChannel();
                var eventType = domainEvent.GetType().Name;
                var routingKey = eventType.ToLowerInvariant();
                var message = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonOptions);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Type = eventType;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            }
            catch (Exception ex)
            {
                // Log error (en producción usar Serilog)
                Console.WriteLine($"Error publicando evento en RabbitMQ: {ex.Message}");
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Libera los recursos utilizados.
        /// </summary>
        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}

