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
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly JsonSerializerOptions _jsonOptions;

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

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar exchange
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
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
                var eventType = domainEvent.GetType().Name;
                var routingKey = eventType.ToLowerInvariant();
                var message = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonOptions);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Type = eventType;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
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

