using System;

namespace events_service.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa la duración de un evento en horas y minutos.
    /// </summary>
    public sealed class DuracionEvento : IEquatable<DuracionEvento>
    {
        /// <summary>
        /// Horas de duración del evento.
        /// </summary>
        public int Horas { get; }

        /// <summary>
        /// Minutos de duración del evento (0-59).
        /// </summary>
        public int Minutos { get; }

        /// <summary>
        /// Total de minutos de duración del evento.
        /// </summary>
        public int TotalMinutos => (Horas * 60) + Minutos;

        /// <summary>
        /// Duración expresada como <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan ComoTimeSpan => TimeSpan.FromMinutes(TotalMinutos);

        /// <summary>
        /// Crea una nueva instancia de DuracionEvento.
        /// </summary>
        /// <param name="horas">Horas de duración. Debe ser mayor o igual a 0.</param>
        /// <param name="minutos">Minutos de duración. Debe estar entre 0 y 59.</param>
        /// <exception cref="ArgumentException">Cuando las horas son negativas, los minutos son negativos, los minutos son mayores a 59, o ambos son cero.</exception>
        public DuracionEvento(int horas, int minutos)
        {
            if (horas < 0)
                throw new ArgumentException("Las horas deben ser una cantidad positiva.", nameof(horas));

            if (minutos < 0)
                throw new ArgumentException("Los minutos deben ser un valor positivo.", nameof(minutos));

            if (minutos > 59)
                throw new ArgumentException("Los minutos no pueden ser mayores a 59.", nameof(minutos));

            if (horas == 0 && minutos == 0)
                throw new ArgumentException("La duración del evento debe ser mayor a cero.", nameof(horas));

            Horas = horas;
            Minutos = minutos;
        }

        /// <summary>
        /// Compara esta instancia con otro objeto.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is DuracionEvento other && Equals(other);
        }

        /// <summary>
        /// Compara esta instancia con otra DuracionEvento.
        /// </summary>
        public bool Equals(DuracionEvento? other)
        {
            if (other is null) return false;
            return Horas == other.Horas && Minutos == other.Minutos;
        }

        /// <summary>
        /// Obtiene el código hash de esta instancia.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Horas, Minutos);
        }

        /// <summary>
        /// Operador de igualdad.
        /// </summary>
        public static bool operator ==(DuracionEvento? left, DuracionEvento? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Operador de desigualdad.
        /// </summary>
        public static bool operator !=(DuracionEvento? left, DuracionEvento? right)
        {
            return !(left == right);
        }
    }
}

