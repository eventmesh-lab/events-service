using System;
using System.Collections.Generic;

namespace events_service.Infrastructure.Persistence
{
    /// <summary>
    /// Entidad de persistencia para el agregado Evento.
    /// Representa la estructura de la tabla eventos en la base de datos.
    /// </summary>
    public class EventoEntity
    {
        /// <summary>
        /// Identificador único del evento.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del evento.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de inicio del evento.
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Duración del evento en horas.
        /// </summary>
        public int DuracionHoras { get; set; }

        /// <summary>
        /// Duración del evento en minutos.
        /// </summary>
        public int DuracionMinutos { get; set; }

        /// <summary>
        /// Estado del evento (Borrador, Publicado, Finalizado, Cancelado).
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Secciones del evento.
        /// </summary>
        public List<SeccionEntity> Secciones { get; set; } = new();

        /// <summary>
        /// Fecha de creación del evento.
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de publicación del evento (opcional).
        /// </summary>
        public DateTime? FechaPublicacion { get; set; }
    }
}

