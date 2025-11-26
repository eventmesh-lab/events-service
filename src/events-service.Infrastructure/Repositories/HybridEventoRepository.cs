using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using events_service.Domain.Entities;
using events_service.Domain.Ports;
using events_service.Infrastructure.Fallback;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace events_service.Infrastructure.Repositories
{
    /// <summary>
    /// Decorador híbrido que intenta persistir con la infraestructura real y, ante fallos, utiliza almacenamiento JSON.
    /// </summary>
    public class HybridEventoRepository : IEventoRepository
    {
        private readonly EventoRepository _primary;
        private readonly IEventoFallbackStore _fallbackStore;
        private readonly ILogger<HybridEventoRepository> _logger;

        public HybridEventoRepository(EventoRepository primary, IEventoFallbackStore fallbackStore, ILogger<HybridEventoRepository> logger)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _fallbackStore = fallbackStore ?? throw new ArgumentNullException(nameof(fallbackStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddAsync(Evento evento, CancellationToken cancellationToken = default)
        {
            try
            {
                await _primary.AddAsync(evento, cancellationToken);
            }
            catch (Exception ex) when (IsTransientPersistenceFailure(ex))
            {
                _logger.LogWarning(ex, "Fallo al persistir evento {EventoId} en base de datos. Se utilizará fallback JSON.", evento.Id);
                await _fallbackStore.SaveAsync(evento, cancellationToken);
            }
        }

        public async Task<Evento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _primary.GetByIdAsync(id, cancellationToken);
            }
            catch (Exception ex) when (IsTransientPersistenceFailure(ex))
            {
                _logger.LogWarning(ex, "Fallo al consultar evento {EventoId} en base de datos. Se utilizará fallback JSON.", id);
                return await _fallbackStore.GetByIdAsync(id, cancellationToken);
            }
        }

        public async Task UpdateAsync(Evento evento, CancellationToken cancellationToken = default)
        {
            try
            {
                await _primary.UpdateAsync(evento, cancellationToken);
            }
            catch (Exception ex) when (IsTransientPersistenceFailure(ex))
            {
                _logger.LogWarning(ex, "Fallo al actualizar evento {EventoId}. Se intentará guardar en fallback JSON.", evento.Id);
                await _fallbackStore.SaveAsync(evento, cancellationToken);
            }
        }

        private static bool IsTransientPersistenceFailure(Exception ex)
        {
            return ex is DbException or DbUpdateException or TimeoutException or InvalidOperationException;
        }
    }
}
