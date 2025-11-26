using System;

namespace events_service.Infrastructure.Persistence
{
    /// <summary>
    /// Entidad de persistencia para la entidad Seccion.
    /// Representa la estructura de la tabla secciones en la base de datos.
    /// </summary>
    public class SeccionEntity
    {
        /// <summary>
        /// Identificador único de la sección.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del evento al que pertenece la sección.
        /// </summary>
        public Guid EventoId { get; set; }

        /// <summary>
        /// Nombre de la sección.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Precio de entrada para esta sección.
        /// </summary>
        public decimal PrecioMonto { get; set; }

        /// <summary>
        /// Capacidad máxima de la sección.
        /// </summary>
        public int Capacidad { get; set; }

        /// <summary>
        /// Navegación al evento.
        /// </summary>
        public EventoEntity Evento { get; set; } = null!;
    }
}

