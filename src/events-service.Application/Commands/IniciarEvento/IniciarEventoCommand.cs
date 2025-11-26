using System;
using MediatR;

namespace events_service.Application.Commands.IniciarEvento
{
    /// <summary>
    /// Comando para marcar un evento como iniciado cuando llega su fecha programada.
    /// </summary>
    public record IniciarEventoCommand : IRequest
    {
        /// <summary>
        /// Identificador único del evento que debe pasar al estado EnCurso.
        /// </summary>
        public Guid EventoId { get; init; }
    }
}
