using System;
using System.Threading;
using System.Threading.Tasks;

namespace events_service.Domain.Ports;

/// <summary>
/// Puerto para interactuar con el servicio de inscripciones (Registration Service).
/// </summary>
public interface IRegistrationClient
{
    /// <summary>
    /// Consulta la cantidad de inscripciones activas para un evento específico.
    /// </summary>
    /// <param name="eventId">Identificador del evento.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Cantidad de inscripciones.</returns>
    Task<int> GetRegistrationCountAsync(Guid eventId, CancellationToken ct = default);
}
