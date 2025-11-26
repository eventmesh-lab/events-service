using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que indica que un evento ha comenzado.
    /// </summary>
    public class EventoIniciado : IDomainEvent, INotification
    {
        public Guid EventoId { get; }

        public DateTime FechaInicio { get; }

        public DateTime OccurredOn => FechaInicio;

        public EventoIniciado(Guid eventoId, DateTime fechaInicio)
        {
            EventoId = eventoId;
            FechaInicio = fechaInicio;
        }
    }
}
