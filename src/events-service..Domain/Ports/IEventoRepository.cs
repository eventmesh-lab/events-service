using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using events_service.Domain.Entities;

namespace events_service.Domain.Ports
{
    /// <summary>
    /// Interfaz del repositorio para la persistencia de eventos.
    /// Define el contrato para operaciones de persistencia del agregado Evento.
    /// </summary>
    public interface IEventoRepository
    {
        /// <summary>
        /// Agrega un nuevo evento al repositorio.
        /// </summary>
        /// <param name="evento">Evento a agregar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task AddAsync(Evento evento, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un evento por su identificador único.
        /// </summary>
        /// <param name="id">Identificador único del evento.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El evento encontrado, o null si no existe.</returns>
        Task<Evento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un evento existente en el repositorio.
        /// </summary>
        /// <param name="evento">Evento a actualizar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task UpdateAsync(Evento evento, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina físicamente un evento del repositorio.
        /// </summary>
        /// <param name="evento">Evento a eliminar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task DeleteAsync(Evento evento, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los eventos en estado "Publicado".
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos publicados.</returns>
        Task<IReadOnlyList<Evento>> GetPublicadosAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los eventos de un organizador específico.
        /// </summary>
        /// <param name="organizadorId">Identificador único del organizador.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos del organizador.</returns>
        Task<IReadOnlyList<Evento>> GetByOrganizadorIdAsync(Guid organizadorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los eventos de un venue específico.
        /// </summary>
        /// <param name="venueId">Identificador único del venue.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos del venue.</returns>
        Task<IReadOnlyList<Evento>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los eventos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de todos los eventos.</returns>
        Task<IReadOnlyList<Evento>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}

