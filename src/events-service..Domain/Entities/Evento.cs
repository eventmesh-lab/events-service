using System;
using System.Collections.Generic;
using System.Linq;
using events_service.Domain.Events;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Entities
{
    /// <summary>
    /// Agregado raíz que representa un evento.
    /// Gestiona el ciclo de vida completo del evento desde su creación hasta su finalización.
    /// </summary>
    public class Evento
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        private readonly List<Seccion> _secciones = new();

        /// <summary>
        /// Identificador único del evento.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public string Nombre { get; private set; } = string.Empty;

        /// <summary>
        /// Descripción del evento.
        /// </summary>
        public string Descripcion { get; private set; } = string.Empty;

        /// <summary>
        /// Fecha del evento.
        /// </summary>
        public FechaEvento Fecha { get; private set; } = null!;

        /// <summary>
        /// Duración del evento.
        /// </summary>
        public DuracionEvento Duracion { get; private set; } = null!;

        /// <summary>
        /// Estado actual del evento.
        /// </summary>
        public EstadoEvento Estado { get; private set; } = null!;

        /// <summary>
        /// Secciones del evento (colección de solo lectura).
        /// </summary>
        public IReadOnlyCollection<Seccion> Secciones => _secciones.AsReadOnly();

        /// <summary>
        /// Constructor privado para forzar el uso del método factory Crear.
        /// </summary>
        private Evento()
        {
        }

        /// <summary>
        /// Crea un nuevo evento en estado Borrador.
        /// </summary>
        /// <param name="nombre">Nombre del evento. No puede ser nulo o vacío.</param>
        /// <param name="descripcion">Descripción del evento.</param>
        /// <param name="fecha">Fecha del evento.</param>
        /// <param name="duracion">Duración del evento.</param>
        /// <param name="secciones">Lista de secciones del evento. Debe contener al menos una sección.</param>
        /// <returns>Nueva instancia de Evento en estado Borrador.</returns>
        /// <exception cref="ArgumentNullException">Cuando el nombre o las secciones son nulos.</exception>
        /// <exception cref="ArgumentException">Cuando el nombre está vacío o la lista de secciones está vacía.</exception>
        public static Evento Crear(
            string nombre,
            string descripcion,
            DateTime fecha,
            DuracionEvento duracion,
            ICollection<Seccion> secciones)
        {
            if (nombre == null)
                throw new ArgumentNullException(nameof(nombre), "El nombre no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

            if (secciones == null)
                throw new ArgumentNullException(nameof(secciones), "Las secciones no pueden ser nulas.");

            if (secciones.Count == 0)
                throw new ArgumentException("El evento debe tener al menos una sección.", nameof(secciones));

            var evento = new Evento
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = descripcion ?? string.Empty,
                Fecha = new FechaEvento(fecha),
                Duracion = duracion ?? throw new ArgumentNullException(nameof(duracion)),
                Estado = new EstadoEvento("Borrador")
            };

            evento._secciones.AddRange(secciones);

            // Generar evento de dominio
            var eventoCreado = new EventoCreado(evento.Id, evento.Nombre);
            evento._domainEvents.Add(eventoCreado);

            return evento;
        }

        /// <summary>
        /// Publica el evento, cambiando su estado de Borrador a Publicado.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cuando el evento no está en estado Borrador.</exception>
        public void Publicar()
        {
            if (!Estado.EsBorrador)
                throw new InvalidOperationException($"No se puede publicar un evento que está en estado '{Estado.Valor}'. Solo se pueden publicar eventos en estado Borrador.");

            Estado = new EstadoEvento("Publicado");

            // Generar evento de dominio
            var eventoPublicado = new EventoPublicado(Id);
            _domainEvents.Add(eventoPublicado);
        }

        /// <summary>
        /// Agrega una nueva sección al evento.
        /// </summary>
        /// <param name="seccion">Sección a agregar. No puede ser nula.</param>
        /// <exception cref="ArgumentNullException">Cuando la sección es nula.</exception>
        /// <exception cref="InvalidOperationException">Cuando la sección ya existe (duplicada).</exception>
        public void AgregarSeccion(Seccion seccion)
        {
            if (seccion == null)
                throw new ArgumentNullException(nameof(seccion), "La sección no puede ser nula.");

            // Verificar si la sección ya existe (por ID o por nombre)
            if (_secciones.Any(s => s.Id == seccion.Id || s.Nombre == seccion.Nombre))
                throw new InvalidOperationException("No se puede agregar una sección duplicada.");

            _secciones.Add(seccion);
        }

        /// <summary>
        /// Finaliza el evento, cambiando su estado de Publicado a Finalizado.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cuando el evento no está en estado Publicado.</exception>
        public void Finalizar()
        {
            if (!Estado.EsPublicado)
                throw new InvalidOperationException($"No se puede finalizar un evento que está en estado '{Estado.Valor}'. Solo se pueden finalizar eventos en estado Publicado.");

            Estado = new EstadoEvento("Finalizado");
        }

        /// <summary>
        /// Cancela el evento, cambiando su estado a Cancelado.
        /// </summary>
        public void Cancelar()
        {
            if (Estado.EsFinalizado)
                throw new InvalidOperationException($"No se puede cancelar un evento que está en estado '{Estado.Valor}'.");

            Estado = new EstadoEvento("Cancelado");
        }

        /// <summary>
        /// Obtiene todos los eventos de dominio pendientes.
        /// </summary>
        /// <returns>Colección de eventos de dominio.</returns>
        public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();
        }

        /// <summary>
        /// Limpia todos los eventos de dominio pendientes.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}

