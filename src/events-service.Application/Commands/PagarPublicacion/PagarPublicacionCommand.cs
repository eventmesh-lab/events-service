using System;
using MediatR;

namespace events_service.Application.Commands.PagarPublicacion
{
    /// <summary>
    /// Comando que inicia el proceso de pago de publicación de un evento.
    /// </summary>
    public record PagarPublicacionCommand : IRequest
    {
        /// <summary>
        /// Identificador único del evento cuyo pago se registra.
        /// </summary>
        public Guid EventoId { get; init; }

        /// <summary>
        /// Identificador de la transacción de pago iniciada.
        /// </summary>
        public Guid TransaccionPagoId { get; init; }

        /// <summary>
        /// Monto abonado en la transacción. Debe coincidir con la tarifa de publicación configurada.
        /// </summary>
        public decimal Monto { get; init; }
    }
}
