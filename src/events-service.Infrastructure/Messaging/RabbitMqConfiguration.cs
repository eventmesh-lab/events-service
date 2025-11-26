using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using events_service.Domain.Ports;

namespace events_service.Infrastructure.Messaging
{
    /// <summary>
    /// Configuración y registro de servicios de mensajería RabbitMQ.
    /// </summary>
    public static class RabbitMqConfiguration
    {
        /// <summary>
        /// Registra los servicios de mensajería RabbitMQ en el contenedor de dependencias.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public static IServiceCollection AddRabbitMqMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var host = configuration["MessageBroker:Host"] ?? "localhost";
            var exchangeName = configuration["MessageBroker:Exchange"] ?? "eventos.domain.events";

            // Registrar ConnectionFactory
            services.AddSingleton<IConnectionFactory>(sp =>
            {
                return new ConnectionFactory
                {
                    HostName = host,
                    Port = 5672,
                    UserName = configuration["MessageBroker:Username"] ?? "guest",
                    Password = configuration["MessageBroker:Password"] ?? "guest",
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };
            });

            // Registrar RabbitMqEventPublisher como IDomainEventPublisher
            services.AddSingleton<IDomainEventPublisher>(sp =>
            {
                var connectionFactory = sp.GetRequiredService<IConnectionFactory>();
                return new RabbitMqEventPublisher(connectionFactory, exchangeName);
            });

            return services;
        }
    }
}

