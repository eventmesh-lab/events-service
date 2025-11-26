using System;
using System.Collections.Generic;

namespace events_service.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa el estado de un evento.
    /// Gestiona los estados válidos y las transiciones permitidas.
    /// </summary>
    public sealed class EstadoEvento : IEquatable<EstadoEvento>
    {
        private const string Borrador = "Borrador";
        private const string PendientePago = "PendientePago";
        private const string Publicado = "Publicado";
        private const string EnCurso = "EnCurso";
        private const string Finalizado = "Finalizado";
        private const string Cancelado = "Cancelado";

        private static readonly HashSet<string> EstadosValidos = new()
        {
            Borrador,
            PendientePago,
            Publicado,
            EnCurso,
            Finalizado,
            Cancelado
        };

        /// <summary>
        /// Valor del estado del evento.
        /// </summary>
        public string Valor { get; }

        /// <summary>
        /// Indica si el estado es "Borrador".
        /// </summary>
        public bool EsBorrador => Valor == Borrador;

        /// <summary>
        /// Indica si el estado es "Publicado".
        /// </summary>
        public bool EsPublicado => Valor == Publicado;

        /// <summary>
        /// Indica si el estado es "PendientePago".
        /// </summary>
        public bool EsPendientePago => Valor == PendientePago;

        /// <summary>
        /// Indica si el estado es "EnCurso".
        /// </summary>
        public bool EsEnCurso => Valor == EnCurso;

        /// <summary>
        /// Indica si el estado es "Finalizado".
        /// </summary>
        public bool EsFinalizado => Valor == Finalizado;

        /// <summary>
        /// Indica si el estado es "Cancelado".
        /// </summary>
        public bool EsCancelado => Valor == Cancelado;

        /// <summary>
        /// Crea una nueva instancia de EstadoEvento.
        /// </summary>
        /// <param name="estado">Estado del evento. Debe ser uno de los estados válidos: Borrador, Publicado, Finalizado, Cancelado.</param>
        /// <exception cref="ArgumentNullException">Cuando el estado es nulo.</exception>
        /// <exception cref="ArgumentException">Cuando el estado es vacío o no es un estado válido.</exception>
        public EstadoEvento(string estado)
        {
            if (estado == null)
                throw new ArgumentNullException(nameof(estado), "El estado no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("El estado no puede estar vacío.", nameof(estado));

            if (!EstadosValidos.Contains(estado))
                throw new ArgumentException($"El estado '{estado}' no es válido. Los estados válidos son: {string.Join(", ", EstadosValidos)}.", nameof(estado));

            Valor = estado;
        }

        /// <summary>
        /// Valida si es posible transicionar desde este estado a otro estado.
        /// </summary>
        /// <param name="otroEstado">Estado destino de la transición.</param>
        /// <returns>True si la transición es válida, false en caso contrario.</returns>
        public bool PuedeTransicionarA(EstadoEvento otroEstado)
        {
            if (otroEstado == null) return false;

            // Borrador puede transicionar a PendientePago o Cancelado
            if (EsBorrador)
            {
                return otroEstado.EsPendientePago || otroEstado.EsCancelado;
            }

            // Pendiente de pago puede transicionar a Publicado o Cancelado
            if (EsPendientePago)
            {
                return otroEstado.EsPublicado || otroEstado.EsCancelado;
            }

            // Publicado puede transicionar a EnCurso o Cancelado
            if (EsPublicado)
            {
                return otroEstado.EsEnCurso || otroEstado.EsCancelado;
            }

            // En curso puede transicionar a Finalizado o Cancelado
            if (EsEnCurso)
            {
                return otroEstado.EsFinalizado || otroEstado.EsCancelado;
            }

            // Finalizado y Cancelado no pueden transicionar a otros estados
            return false;
        }

        /// <summary>
        /// Compara esta instancia con otro objeto.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is EstadoEvento other && Equals(other);
        }

        /// <summary>
        /// Compara esta instancia con otra EstadoEvento.
        /// </summary>
        public bool Equals(EstadoEvento? other)
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
        public static bool operator ==(EstadoEvento? left, EstadoEvento? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Operador de desigualdad.
        /// </summary>
        public static bool operator !=(EstadoEvento? left, EstadoEvento? right)
        {
            return !(left == right);
        }
    }
}

