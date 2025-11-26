using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que se genera cuando se editan los datos de un evento en estado Borrador.
    /// </summary>
    public class EventoEditado : IDomainEvent, INotification
    {
        /// <summary>
        /// Identificador del evento editado.
        /// </summary>
        public Guid EventoId { get; }

        /// <summary>
        /// Fecha y hora en que se realizó la edición.
        /// </summary>
        public DateTime FechaEdicion { get; }

        public DateTime OccurredOn => FechaEdicion;

        public EventoEditado(Guid eventoId, DateTime fechaEdicion)
        {
            EventoId = eventoId;
            FechaEdicion = fechaEdicion;
        }
    }
}
