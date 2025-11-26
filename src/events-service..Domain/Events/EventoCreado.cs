using System;
using MediatR;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Evento de dominio que se genera cuando se crea un nuevo evento.
    /// </summary>
    public class EventoCreado : IDomainEvent, INotification
    {
        /// <summary>
        /// Identificador único del evento creado.
        /// </summary>
        public Guid EventoId { get; }

        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Identificador del organizador propietario del evento.
        /// </summary>
        public Guid OrganizadorId { get; }

        /// <summary>
        /// Fecha programada de inicio del evento.
        /// </summary>
        public DateTime FechaInicio { get; }

        /// <summary>
        /// Fecha y hora en que se registró la creación del evento.
        /// </summary>
        public DateTime FechaCreacion { get; }

        public DateTime OccurredOn => FechaCreacion;

        /// <summary>
        /// Crea una nueva instancia de EventoCreado.
        /// </summary>
        /// <param name="eventoId">Identificador único del evento.</param>
        /// <param name="nombre">Nombre del evento.</param>
        /// <param name="organizadorId">Identificador del organizador.</param>
        /// <param name="fechaInicio">Fecha de inicio del evento.</param>
        /// <param name="fechaCreacion">Fecha de creación del evento.</param>
        public EventoCreado(Guid eventoId, string nombre, Guid organizadorId, DateTime fechaInicio, DateTime fechaCreacion)
        {
            EventoId = eventoId;
            Nombre = nombre;
            OrganizadorId = organizadorId;
            FechaInicio = fechaInicio;
            FechaCreacion = fechaCreacion;
        }
    }
}

