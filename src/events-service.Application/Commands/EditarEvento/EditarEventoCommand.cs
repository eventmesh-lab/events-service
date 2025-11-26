using System;
using System.Collections.Generic;
using MediatR;

namespace events_service.Application.Commands.EditarEvento
{
    /// <summary>
    /// Comando encargado de editar un evento existente que permanece en estado Borrador.
    /// Actualiza los datos generales del evento junto con sus secciones asociadas.
    /// </summary>
    public record EditarEventoCommand : IRequest
    {
        /// <summary>
        /// Identificador único del evento que se desea editar.
        /// </summary>
        public Guid EventoId { get; init; }

        /// <summary>
        /// Nombre actualizado del evento.
        /// </summary>
        public string Nombre { get; init; } = string.Empty;

        /// <summary>
        /// Descripción del evento.
        /// </summary>
        public string Descripcion { get; init; } = string.Empty;

        /// <summary>
        /// Nueva fecha programada del evento.
        /// </summary>
        public DateTime Fecha { get; init; }

        /// <summary>
        /// Horas que compondrán la nueva duración del evento.
        /// </summary>
        public int HorasDuracion { get; init; }

        /// <summary>
        /// Minutos adicionales para la duración del evento.
        /// </summary>
        public int MinutosDuracion { get; init; }

        /// <summary>
        /// Categoría a la que pertenece el evento.
        /// </summary>
        public string Categoria { get; init; } = string.Empty;

        /// <summary>
        /// Colección con las secciones que quedarán asociadas al evento.
        /// </summary>
        public List<SeccionDto> Secciones { get; init; } = new();

        /// <summary>
        /// DTO que representa la información de una sección dentro del comando de edición.
        /// </summary>
        public record SeccionDto
        {
            /// <summary>
            /// Identificador de la sección. Se utiliza para preservar secciones existentes.
            /// Cuando no se proporciona se generará un nuevo identificador.
            /// </summary>
            public Guid? Id { get; init; }

            /// <summary>
            /// Nombre descriptivo de la sección.
            /// </summary>
            public string Nombre { get; init; } = string.Empty;

            /// <summary>
            /// Capacidad total de asistentes permitidos en la sección.
            /// </summary>
            public int Capacidad { get; init; }

            /// <summary>
            /// Precio base que se cobrará por cada entrada de la sección.
            /// </summary>
            public decimal Precio { get; init; }
        }
    }
}
