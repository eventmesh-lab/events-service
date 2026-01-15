using System;
using System.Collections.Generic;
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

        public async Task DeleteAsync(Evento evento, CancellationToken cancellationToken = default)
        {
            // La eliminación física se intenta solo en el repositorio primario.
            // Si falla el primario por problemas transitorios, se asume que no se puede realizar la operación real.
            await _primary.DeleteAsync(evento, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los eventos en estado "Publicado".
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos publicados, o lista vacía si falla.</returns>
        public async Task<IReadOnlyList<Evento>> GetPublicadosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _primary.GetPublicadosAsync(cancellationToken);
            }
            catch (Exception ex) when (IsTransientPersistenceFailure(ex))
            {
                _logger.LogWarning(ex, "Fallo al consultar eventos publicados en base de datos. Se retornará lista vacía.");
                return Array.Empty<Evento>();
            }
        }

        /// <summary>
        /// Obtiene todos los eventos de un organizador específico.
        /// </summary>
        /// <param name="organizadorId">Identificador único del organizador.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos del organizador, o lista vacía si falla.</returns>
        public async Task<IReadOnlyList<Evento>> GetByOrganizadorIdAsync(Guid organizadorId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _primary.GetByOrganizadorIdAsync(organizadorId, cancellationToken);
            }
            catch (Exception ex) when (IsTransientPersistenceFailure(ex))
            {
                _logger.LogWarning(ex, "Fallo al consultar eventos del organizador {OrganizadorId} en base de datos. Se retornará lista vacía.", organizadorId);
                return Array.Empty<Evento>();
            }
        }

        /// <summary>
        /// Obtiene todos los eventos de un venue específico.
        /// </summary>
        /// <param name="venueId">Identificador único del venue.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos del venue, o lista vacía si falla.</returns>
        public async Task<IReadOnlyList<Evento>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _primary.GetByVenueIdAsync(venueId, cancellationToken);
            }
            catch (Exception ex) when (IsTransientPersistenceFailure(ex))
            {
                _logger.LogWarning(ex, "Fallo al consultar eventos del venue {VenueId} en base de datos. Se retornará lista vacía.", venueId);
                return Array.Empty<Evento>();
            }
        }

        /// <summary>
        /// Obtiene todos los eventos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de todos los eventos, o lista vacía si falla.</returns>
        public async Task<IReadOnlyList<Evento>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _primary.GetAllAsync(cancellationToken);
            }
            catch (Exception ex) when (IsTransientPersistenceFailure(ex))
            {
                _logger.LogWarning(ex, "Fallo al consultar todos los eventos en base de datos. Se retornará lista vacía.");
                return Array.Empty<Evento>();
            }
        }

        private static bool IsTransientPersistenceFailure(Exception ex)
        {
            return ex is DbException or DbUpdateException or TimeoutException or InvalidOperationException;
        }
    }
}
