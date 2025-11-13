using System;
using System.Collections.Generic;
using System.Linq;
using events_service.Domain.Entities;
using events_service.Domain.ValueObjects;

namespace events_service.Api.DTOs
{
    /// <summary>
    /// DTO de respuesta para representar un evento en las respuestas de la API.
    /// </summary>
    public record EventoResponseDto
    {
        /// <summary>
        /// Identificador único del evento.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public string Nombre { get; init; } = string.Empty;

        /// <summary>
        /// Descripción del evento.
        /// </summary>
        public string Descripcion { get; init; } = string.Empty;

        /// <summary>
        /// Fecha programada del evento.
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
        /// Estado actual del evento (Borrador, PendientePago, Publicado, EnCurso, Finalizado, Cancelado).
        /// </summary>
        public string Estado { get; init; } = string.Empty;

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
        /// Identificador de la transacción de pago (opcional).
        /// </summary>
        public Guid? TransaccionPagoId { get; init; }

        /// <summary>
        /// Fecha de creación del evento.
        /// </summary>
        public DateTime FechaCreacion { get; init; }

        /// <summary>
        /// Fecha de publicación del evento (opcional).
        /// </summary>
        public DateTime? FechaPublicacion { get; init; }

        /// <summary>
        /// Versión del agregado para control de concurrencia optimista.
        /// </summary>
        public int Version { get; init; }

        /// <summary>
        /// Lista de secciones del evento.
        /// </summary>
        public List<SeccionResponseDto> Secciones { get; init; } = new();

        /// <summary>
        /// Mapea un agregado de dominio Evento a un DTO de respuesta.
        /// </summary>
        public static EventoResponseDto FromDomain(Evento evento)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            return new EventoResponseDto
            {
                Id = evento.Id,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                Fecha = evento.Fecha.Valor,
                HorasDuracion = evento.Duracion.Horas,
                MinutosDuracion = evento.Duracion.Minutos,
                Estado = evento.Estado.Valor,
                OrganizadorId = evento.OrganizadorId,
                VenueId = evento.VenueId,
                Categoria = evento.Categoria,
                TarifaPublicacion = evento.TarifaPublicacion,
                TransaccionPagoId = evento.TransaccionPagoId,
                FechaCreacion = evento.FechaCreacion,
                FechaPublicacion = evento.FechaPublicacion,
                Version = evento.Version,
                Secciones = evento.Secciones.Select(s => SeccionResponseDto.FromDomain(s)).ToList()
            };
        }
    }

    /// <summary>
    /// DTO de respuesta para representar una sección de un evento.
    /// </summary>
    public record SeccionResponseDto
    {
        /// <summary>
        /// Identificador único de la sección.
        /// </summary>
        public Guid Id { get; init; }

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
        /// Mapea una entidad de dominio Seccion a un DTO de respuesta.
        /// </summary>
        public static SeccionResponseDto FromDomain(Seccion seccion)
        {
            if (seccion == null)
                throw new ArgumentNullException(nameof(seccion));

            return new SeccionResponseDto
            {
                Id = seccion.Id,
                Nombre = seccion.Nombre,
                Capacidad = seccion.Capacidad,
                Precio = seccion.Precio.Valor
            };
        }
    }
}


