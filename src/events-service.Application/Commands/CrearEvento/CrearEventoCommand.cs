using System;
using System.Collections.Generic;
using MediatR;

namespace events_service.Application.Commands.CrearEvento
{
    /// <summary>
    /// Comando para crear un nuevo evento en estado borrador.
    /// </summary>
    public record CrearEventoCommand : IRequest<CrearEventoCommandResponse>
    {
        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public string Nombre { get; init; } = string.Empty;

        /// <summary>
        /// Descripción del evento.
        /// </summary>
        public string Descripcion { get; init; } = string.Empty;

        /// <summary>
        /// Fecha de inicio del evento.
        /// </summary>
        public DateTime Fecha { get; init; }

        /// <summary>
        /// Horas de duración del evento.
        /// </summary>
        public int HorasDuracion { get; init; }

        /// <summary>
        /// Minutos de duración del evento.
        /// </summary>
        public int MinutosDuracion { get; init; }

        /// <summary>
        /// Lista de secciones del evento.
        /// </summary>
        public List<SeccionDto> Secciones { get; init; } = new();

        /// <summary>
        /// Identificador del organizador propietario del evento.
        /// </summary>
        public Guid OrganizadorId { get; init; }

        /// <summary>
        /// Identificador del venue donde se realizará el evento.
        /// </summary>
        public Guid VenueId { get; init; }

        /// <summary>
        /// Categoría del evento.
        /// </summary>
        public string Categoria { get; init; } = string.Empty;

        /// <summary>
        /// Tarifa de publicación configurada para el evento.
        /// </summary>
        public decimal TarifaPublicacion { get; init; }

        /// <summary>
        /// DTO que representa una sección del evento.
        /// </summary>
        public record SeccionDto
        {
            /// <summary>
            /// Nombre de la sección.
            /// </summary>
            public string Nombre { get; init; } = string.Empty;

            /// <summary>
            /// Capacidad máxima de la sección.
            /// </summary>
            public int Capacidad { get; init; }

            /// <summary>
            /// Precio de entrada para esta sección.
            /// </summary>
            public decimal Precio { get; init; }

            /// <summary>
            /// Tipo de asiento de la sección (opcional).
            /// </summary>
            public string? TipoAsiento { get; init; }
        }
    }

    /// <summary>
    /// Respuesta del comando CrearEventoCommand.
    /// </summary>
    public record CrearEventoCommandResponse
    {
        /// <summary>
        /// Identificador único del evento creado.
        /// </summary>
        public Guid Id { get; init; }
    }
}

