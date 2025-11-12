using System.Threading;
using System.Threading.Tasks;
using events_service.Domain.Events;

namespace events_service.Infrastructure.Messaging
{
    /// <summary>
    /// Interfaz para publicar eventos de dominio en el bus de mensajería.
    /// </summary>
    public interface IDomainEventPublisher
    {
        /// <summary>
        /// Publica un evento de dominio en el bus de mensajería.
        /// </summary>
        /// <param name="domainEvent">Evento de dominio a publicar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
}

