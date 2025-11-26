using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio emitido cuando un evento se cancela.
    /// </summary>
    public class EventoCancelado : IDomainEvent, INotification
    {
        public Guid EventoId { get; }

        public string Motivo { get; }

        public DateTime OccurredOn { get; }

        public EventoCancelado(Guid eventoId, string motivo)
        {
            EventoId = eventoId;
            Motivo = motivo;
            OccurredOn = DateTime.Now;
        }
    }
}
