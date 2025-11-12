using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que se genera cuando se crea un nuevo evento.
    /// </summary>
    public class EventoCreado : IDomainEvent, INotification
    {
        /// <summary>
        /// Identificador único del evento creado.
        /// </summary>
        public Guid EventoId { get; }

        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Crea una nueva instancia de EventoCreado.
        /// </summary>
        /// <param name="eventoId">Identificador único del evento.</param>
        /// <param name="nombre">Nombre del evento.</param>
        public EventoCreado(Guid eventoId, string nombre)
        {
            EventoId = eventoId;
            Nombre = nombre;
            OccurredOn = DateTime.UtcNow;
        }
    }
}

