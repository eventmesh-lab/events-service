using System;
using MediatR;

namespace events_service.Application.Commands.PublicarEvento
{
    /// <summary>
    /// Comando para publicar un evento existente.
    /// Cambia el estado del evento de Borrador a Publicado.
    /// </summary>
    public record PublicarEventoCommand : IRequest
    {
        /// <summary>
        /// Identificador único del evento a publicar.
        /// </summary>
        public Guid EventoId { get; init; }

        /// <summary>
        /// Identificador del pago confirmado asociado al evento.
        /// </summary>
        public Guid PagoConfirmadoId { get; init; }
    }
}

