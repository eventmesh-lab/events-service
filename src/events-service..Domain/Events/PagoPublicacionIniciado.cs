using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que representa que se inició el proceso de pago de publicación.
    /// </summary>
    public class PagoPublicacionIniciado : IDomainEvent, INotification
    {
        public Guid EventoId { get; }

        public Guid TransaccionPagoId { get; }

        public decimal Monto { get; }

        public DateTime OccurredOn { get; }

        public PagoPublicacionIniciado(Guid eventoId, Guid transaccionPagoId, decimal monto)
        {
            EventoId = eventoId;
            TransaccionPagoId = transaccionPagoId;
            Monto = monto;
            OccurredOn = DateTime.Now;
        }
    }
}
