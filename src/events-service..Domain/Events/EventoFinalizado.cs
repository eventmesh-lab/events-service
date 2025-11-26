using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que indica que un evento ha finalizado.
    /// </summary>
    public class EventoFinalizado : IDomainEvent, INotification
    {
        public Guid EventoId { get; }

        public DateTime FechaFinalizacion { get; }

        public DateTime OccurredOn => FechaFinalizacion;

        public EventoFinalizado(Guid eventoId, DateTime fechaFinalizacion)
        {
            EventoId = eventoId;
            FechaFinalizacion = fechaFinalizacion;
        }
    }
}
