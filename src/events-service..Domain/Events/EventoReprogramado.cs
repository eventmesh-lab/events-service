using System;

namespace events_service.Domain.Events;

/// <summary>
/// Evento de dominio que se genera cuando un evento es reprogramado.
/// </summary>
/// <param name="EventoId">Identificador único del evento.</param>
/// <param name="NuevaFecha">Nueva fecha programada.</param>
/// <param name="ReprogramadoPor">Identificador del usuario que realizó la reprogramación.</param>
/// <param name="OccurredOn">Fecha y hora en que ocurrió el cambio.</param>
public record EventoReprogramado(
    Guid EventoId,
    DateTime NuevaFecha,
    string ReprogramadoPor,
    DateTime OccurredOn) : IDomainEvent;
