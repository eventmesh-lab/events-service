using System;

namespace events_service.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa la fecha de un evento.
    /// Valida que la fecha sea válida y no esté en el pasado.
    /// </summary>
    public sealed class FechaEvento : IEquatable<FechaEvento>
    {
        /// <summary>
        /// Fecha programada del evento.
        /// </summary>
        public DateTime Valor { get; }

        /// <summary>
        /// Constructor privado para reconstrucción desde persistencia sin validación de fecha pasada.
        /// </summary>
        private FechaEvento(DateTime fecha, bool skipValidation)
        {
            Valor = fecha.Date;
        }

        /// <summary>
        /// Crea una nueva instancia de FechaEvento.
        /// </summary>
        /// <param name="fecha">Fecha del evento. Debe ser una fecha válida y no estar en el pasado.</param>
        /// <exception cref="ArgumentNullException">Cuando la fecha es nula.</exception>
        /// <exception cref="ArgumentException">Cuando la fecha está en el pasado.</exception>
        public FechaEvento(DateTime? fecha)
        {
            if (fecha == null)
                throw new ArgumentNullException(nameof(fecha), "La fecha no puede ser nula.");

            var fechaDate = fecha.Value.Date;
            var hoy = DateTime.Now.Date;

            if (fechaDate < hoy)
                throw new ArgumentException("La fecha del evento debe ser hoy o en el futuro.", nameof(fecha));

            Valor = fechaDate;
        }

        /// <summary>
        /// Reconstruye una instancia de FechaEvento desde persistencia sin validar que la fecha no esté en el pasado.
        /// Este método debe usarse únicamente cuando se reconstruye un evento desde la base de datos.
        /// </summary>
        /// <param name="fecha">Fecha del evento desde persistencia.</param>
        /// <returns>Instancia de FechaEvento sin validación de fecha pasada.</returns>
        /// <exception cref="ArgumentNullException">Cuando la fecha es nula.</exception>
        public static FechaEvento ReconstruirDesdePersistencia(DateTime? fecha)
        {
            if (fecha == null)
                throw new ArgumentNullException(nameof(fecha), "La fecha no puede ser nula.");

            return new FechaEvento(fecha.Value, skipValidation: true);
        }

        /// <summary>
        /// Compara esta instancia con otro objeto.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is FechaEvento other && Equals(other);
        }

        /// <summary>
        /// Compara esta instancia con otra FechaEvento.
        /// </summary>
        public bool Equals(FechaEvento? other)
        {
            if (other is null) return false;
            return Valor.Date == other.Valor.Date;
        }

        /// <summary>
        /// Obtiene el código hash de esta instancia.
        /// </summary>
        public override int GetHashCode()
        {
            return Valor.Date.GetHashCode();
        }

        /// <summary>
        /// Indica si la fecha del evento ya ocurrió con base en una fecha de referencia.
        /// </summary>
        public bool HaComenzado(DateTime fechaReferencia)
        {
            return fechaReferencia.Date >= Valor.Date;
        }

        /// <summary>
        /// Operador de igualdad.
        /// </summary>
        public static bool operator ==(FechaEvento? left, FechaEvento? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Operador de desigualdad.
        /// </summary>
        public static bool operator !=(FechaEvento? left, FechaEvento? right)
        {
            return !(left == right);
        }
    }
}

