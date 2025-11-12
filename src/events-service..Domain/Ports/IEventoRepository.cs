using System;
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
    }
}

