using System;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Entities
{
    /// <summary>
    /// Entidad que representa una sección de un evento.
    /// </summary>
    public class Seccion
    {
        /// <summary>
        /// Identificador único de la sección.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Nombre de la sección.
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Capacidad máxima de la sección.
        /// </summary>
        public int Capacidad { get; }

        /// <summary>
        /// Precio de entrada para esta sección.
        /// </summary>
        public PrecioEntrada Precio { get; }

        /// <summary>
        /// Crea una nueva instancia de Seccion con un ID generado automáticamente.
        /// </summary>
        /// <param name="nombre">Nombre de la sección. No puede ser nulo o vacío.</param>
        /// <param name="capacidad">Capacidad máxima de la sección. Debe ser mayor a cero.</param>
        /// <param name="precio">Precio de entrada para esta sección. No puede ser nulo.</param>
        /// <exception cref="ArgumentNullException">Cuando el nombre o el precio son nulos.</exception>
        /// <exception cref="ArgumentException">Cuando el nombre está vacío o la capacidad es menor o igual a cero.</exception>
        public Seccion(string nombre, int capacidad, PrecioEntrada precio)
            : this(Guid.NewGuid(), nombre, capacidad, precio)
        {
        }

        /// <summary>
        /// Crea una nueva instancia de Seccion con un ID específico.
        /// </summary>
        /// <param name="id">Identificador único de la sección.</param>
        /// <param name="nombre">Nombre de la sección. No puede ser nulo o vacío.</param>
        /// <param name="capacidad">Capacidad máxima de la sección. Debe ser mayor a cero.</param>
        /// <param name="precio">Precio de entrada para esta sección. No puede ser nulo.</param>
        /// <exception cref="ArgumentNullException">Cuando el nombre o el precio son nulos.</exception>
        /// <exception cref="ArgumentException">Cuando el nombre está vacío o la capacidad es menor o igual a cero.</exception>
        public Seccion(Guid id, string nombre, int capacidad, PrecioEntrada precio)
        {
            if (nombre == null)
                throw new ArgumentNullException(nameof(nombre), "El nombre no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

            if (capacidad <= 0)
                throw new ArgumentException("La capacidad debe ser mayor a cero.", nameof(capacidad));

            if (precio == null)
                throw new ArgumentNullException(nameof(precio), "El precio no puede ser nulo.");

            Id = id;
            Nombre = nombre;
            Capacidad = capacidad;
            Precio = precio;
        }

        /// <summary>
        /// Compara esta instancia con otro objeto basándose en el ID.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Seccion other && Id == other.Id;
        }

        /// <summary>
        /// Obtiene el código hash basado en el ID.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

