using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que se genera cuando se publica un evento.
    /// </summary>
    public class EventoPublicado : IDomainEvent, INotification
    {
        /// <summary>
        /// Identificador único del evento publicado.
        /// </summary>
        public Guid EventoId { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Crea una nueva instancia de EventoPublicado.
        /// </summary>
        /// <param name="eventoId">Identificador único del evento.</param>
        public EventoPublicado(Guid eventoId)
        {
            EventoId = eventoId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}

