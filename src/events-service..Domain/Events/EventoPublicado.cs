using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que se genera cuando se publica un evento.
    /// </summary>
    public class EventoPublicado : IDomainEvent, INotification
    {
        /// <summary>
        /// Identificador único del evento publicado.
        /// </summary>
        public Guid EventoId { get; }

        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Identificador del organizador propietario.
        /// </summary>
        public Guid OrganizadorId { get; }

        /// <summary>
        /// Fecha de publicación del evento.
        /// </summary>
        public DateTime FechaPublicacion { get; }

        public DateTime OccurredOn => FechaPublicacion;

        /// <summary>
        /// Crea una nueva instancia de EventoPublicado.
        /// </summary>
        /// <param name="eventoId">Identificador único del evento.</param>
        /// <param name="nombre">Nombre del evento.</param>
        /// <param name="organizadorId">Identificador del organizador.</param>
        /// <param name="fechaPublicacion">Fecha de publicación.</param>
        public EventoPublicado(Guid eventoId, string nombre, Guid organizadorId, DateTime fechaPublicacion)
        {
            EventoId = eventoId;
            Nombre = nombre;
            OrganizadorId = organizadorId;
            FechaPublicacion = fechaPublicacion;
        }
    }
}

