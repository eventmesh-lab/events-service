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
        /// Identificador del organizador propietario del evento.
        /// </summary>
        public Guid OrganizadorId { get; set; }

        /// <summary>
        /// Identificador del venue donde se realizará el evento.
        /// </summary>
        public Guid VenueId { get; set; }

        /// <summary>
        /// Categoría del evento.
        /// </summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>
        /// Tarifa configurada para la publicación del evento.
        /// </summary>
        public decimal TarifaPublicacion { get; set; }

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
        /// Identificador de la transacción de pago registrada (opcional).
        /// </summary>
        public Guid? TransaccionPagoId { get; set; }

        /// <summary>
        /// Versión del agregado para control de concurrencia optimista.
        /// </summary>
        public int Version { get; set; }

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

        /// <summary>
        /// Blob de la imagen principal.
        /// </summary>
        public string? ImagenPrincipalBlobName { get; set; }

        public string? ImagenPrincipalContentType { get; set; }

        public long? ImagenPrincipalSizeBytes { get; set; }

        /// <summary>
        /// Lista de blobs de imágenes secundarias en formato JSON.
        /// </summary>
        public string ImagenesSecundariasJson { get; set; } = "[]";

        /// <summary>
        /// Blob del folleto en PDF.
        /// </summary>
        public string? FolletoBlobName { get; set; }

        public string? FolletoContentType { get; set; }

        public long? FolletoSizeBytes { get; set; }

        // Nuevos campos para cancelación y reprogramación
        public string? MotivoCancelacion { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public string? CanceladoPor { get; set; }
        public DateTime? FechaInicioOriginal { get; set; }
        public DateTime? FechaFinOriginal { get; set; }
        public int ContadorReprogramaciones { get; set; }
        public DateTime? UltimaReprogramacionFecha { get; set; }
        public string? UltimaReprogramacionPor { get; set; }
    }
}

