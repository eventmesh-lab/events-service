using System;
using System.Threading;
using System.Threading.Tasks;
using events_service.Domain.Entities;

namespace events_service.Infrastructure.Fallback
{
    /// <summary>
    /// Contrato para almacenar y recuperar eventos cuando la persistencia primaria falla.
    /// </summary>
    public interface IEventoFallbackStore
    {
        /// <summary>
        /// Persiste un evento en el mecanismo de respaldo.
        /// </summary>
        Task SaveAsync(Evento evento, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un evento desde el respaldo por su identificador.
        /// </summary>
        Task<Evento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
