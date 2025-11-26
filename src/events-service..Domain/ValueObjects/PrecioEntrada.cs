using System;
using System.Globalization;

namespace events_service.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa el precio de entrada a un evento.
    /// </summary>
    public sealed class PrecioEntrada : IEquatable<PrecioEntrada>
    {
        /// <summary>
        /// Valor del precio de entrada.
        /// </summary>
        public decimal Valor { get; }

        /// <summary>
        /// Indica si la entrada es gratis (precio es cero).
        /// </summary>
        public bool EsGratis => Valor == 0;

        /// <summary>
        /// Crea una nueva instancia de PrecioEntrada.
        /// </summary>
        /// <param name="precio">Precio de entrada. Debe ser mayor o igual a cero.</param>
        /// <exception cref="ArgumentException">Cuando el precio es negativo.</exception>
        public PrecioEntrada(decimal precio)
        {
            if (precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.", nameof(precio));

            // Redondear a 2 decimales
            Valor = Math.Round(precio, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Compara esta instancia con otro objeto.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is PrecioEntrada other && Equals(other);
        }

        /// <summary>
        /// Compara esta instancia con otra PrecioEntrada.
        /// </summary>
        public bool Equals(PrecioEntrada? other)
        {
            if (other is null) return false;
            return Valor == other.Valor;
        }

        /// <summary>
        /// Obtiene el código hash de esta instancia.
        /// </summary>
        public override int GetHashCode()
        {
            return Valor.GetHashCode();
        }

        /// <summary>
        /// Operador de igualdad.
        /// </summary>
        public static bool operator ==(PrecioEntrada? left, PrecioEntrada? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Operador de desigualdad.
        /// </summary>
        public static bool operator !=(PrecioEntrada? left, PrecioEntrada? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Retorna una representación en string del precio formateado como moneda.
        /// </summary>
        public override string ToString()
        {
            return Valor.ToString("C", CultureInfo.CurrentCulture);
        }
    }
}

