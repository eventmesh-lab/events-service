using System;

namespace events_service.Domain.Events
{
    /// <summary>
    /// Interfaz base para eventos de dominio.
    /// Los eventos de dominio representan algo que ha sucedido en el dominio y deben ser inmutables.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento.
        /// </summary>
        DateTime OccurredOn { get; }
    }
}

