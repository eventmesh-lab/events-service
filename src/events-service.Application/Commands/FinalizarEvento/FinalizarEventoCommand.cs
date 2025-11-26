using System;
using MediatR;

namespace events_service.Application.Commands.FinalizarEvento
{
    /// <summary>
    /// Comando para finalizar un evento que se encuentra en curso.
    /// </summary>
    public record FinalizarEventoCommand : IRequest
    {
        /// <summary>
        /// Identificador del evento que se debe marcar como finalizado.
        /// </summary>
        public Guid EventoId { get; init; }
    }
}
