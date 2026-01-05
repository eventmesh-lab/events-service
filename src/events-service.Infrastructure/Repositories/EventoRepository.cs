using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using events_service.Domain.Entities;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;
using events_service.Infrastructure.Persistence;

namespace events_service.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de eventos usando Entity Framework Core.
    /// Persiste y recupera eventos desde PostgreSQL.
    /// </summary>
    public class EventoRepository : IEventoRepository
    {
        private readonly EventsDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio.
        /// </summary>
        /// <param name="context">Contexto de base de datos.</param>
        public EventoRepository(EventsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Agrega un nuevo evento al repositorio.
        /// </summary>
        /// <param name="evento">Evento a agregar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task AddAsync(Evento evento, CancellationToken cancellationToken = default)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            var entity = MapToEntity(evento);
            entity.FechaCreacion = evento.FechaCreacion;

            await _context.Eventos.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene un evento por su identificador único.
        /// </summary>
        /// <param name="id">Identificador único del evento.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El evento encontrado, o null si no existe.</returns>
        public async Task<Evento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Eventos
                .Include(e => e.Secciones)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity == null)
                return null;

            return MapToDomain(entity);
        }

        /// <summary>
        /// Actualiza un evento existente en el repositorio.
        /// </summary>
        /// <param name="evento">Evento a actualizar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task UpdateAsync(Evento evento, CancellationToken cancellationToken = default)
        {
            if (evento == null)
                throw new ArgumentNullException(nameof(evento));

            var entity = await _context.Eventos
                .Include(e => e.Secciones)
                .FirstOrDefaultAsync(e => e.Id == evento.Id, cancellationToken);

            if (entity == null)
                throw new InvalidOperationException($"El evento con ID {evento.Id} no existe.");

            // Actualizar propiedades
            entity.Nombre = evento.Nombre;
            entity.Descripcion = evento.Descripcion;
            entity.OrganizadorId = evento.OrganizadorId;
            entity.VenueId = evento.VenueId;
            entity.Categoria = evento.Categoria;
            entity.TarifaPublicacion = evento.TarifaPublicacion;
            entity.FechaInicio = evento.Fecha.Valor;
            entity.DuracionHoras = evento.Duracion.Horas;
            entity.DuracionMinutos = evento.Duracion.Minutos;
            entity.Estado = evento.Estado.Valor;
            entity.TransaccionPagoId = evento.TransaccionPagoId;
            entity.Version = evento.Version;
            entity.FechaCreacion = evento.FechaCreacion;
            entity.FechaPublicacion = evento.FechaPublicacion;
            entity.ImagenPrincipalBlobName = evento.ImagenPrincipal?.BlobName;
            entity.ImagenPrincipalContentType = evento.ImagenPrincipal?.ContentType;
            entity.ImagenPrincipalSizeBytes = evento.ImagenPrincipal?.SizeBytes;
            entity.ImagenesSecundariasJson = SerializarSecundarias(evento.ImagenesSecundarias);
            entity.FolletoBlobName = evento.FolletoPdf?.BlobName;
            entity.FolletoContentType = evento.FolletoPdf?.ContentType;
            entity.FolletoSizeBytes = evento.FolletoPdf?.SizeBytes;

            // Actualizar secciones (eliminar las que ya no existen y agregar nuevas)
            var seccionesExistentes = entity.Secciones.ToList();
            var seccionesDominio = evento.Secciones.ToList();

            // Eliminar secciones que ya no están en el dominio
            var seccionesAEliminar = seccionesExistentes
                .Where(s => !seccionesDominio.Any(sd => sd.Id == s.Id))
                .ToList();
            foreach (var seccion in seccionesAEliminar)
            {
                _context.Secciones.Remove(seccion);
            }

            // Actualizar o agregar secciones
            foreach (var seccionDominio in seccionesDominio)
            {
                var seccionEntity = seccionesExistentes.FirstOrDefault(s => s.Id == seccionDominio.Id);
                if (seccionEntity == null)
                {
                    // Agregar nueva sección
                    seccionEntity = new SeccionEntity
                    {
                        Id = seccionDominio.Id,
                        EventoId = evento.Id,
                        Nombre = seccionDominio.Nombre,
                        Capacidad = seccionDominio.Capacidad,
                        PrecioMonto = seccionDominio.Precio.Valor
                    };
                    _context.Secciones.Add(seccionEntity);
                }
                else
                {
                    // Actualizar sección existente
                    seccionEntity.Nombre = seccionDominio.Nombre;
                    seccionEntity.Capacidad = seccionDominio.Capacidad;
                    seccionEntity.PrecioMonto = seccionDominio.Precio.Valor;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los eventos en estado "Publicado".
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos publicados.</returns>
        public async Task<IReadOnlyList<Evento>> GetPublicadosAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _context.Eventos
                .Include(e => e.Secciones)
                .Where(e => e.Estado == "Publicado")
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDomain).ToList();
        }

        /// <summary>
        /// Obtiene todos los eventos de un organizador específico.
        /// </summary>
        /// <param name="organizadorId">Identificador único del organizador.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos del organizador.</returns>
        public async Task<IReadOnlyList<Evento>> GetByOrganizadorIdAsync(Guid organizadorId, CancellationToken cancellationToken = default)
        {
            var entities = await _context.Eventos
                .Include(e => e.Secciones)
                .Where(e => e.OrganizadorId == organizadorId)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDomain).ToList();
        }

        /// <summary>
        /// Obtiene todos los eventos de un venue específico.
        /// </summary>
        /// <param name="venueId">Identificador único del venue.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de eventos del venue.</returns>
        public async Task<IReadOnlyList<Evento>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
        {
            var entities = await _context.Eventos
                .Include(e => e.Secciones)
                .Where(e => e.VenueId == venueId)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDomain).ToList();
        }

        /// <summary>
        /// Obtiene todos los eventos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de todos los eventos.</returns>
        public async Task<IReadOnlyList<Evento>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _context.Eventos
                .Include(e => e.Secciones)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToDomain).ToList();
        }

        /// <summary>
        /// Mapea una entidad de persistencia a un agregado de dominio.
        /// </summary>
        private Evento MapToDomain(EventoEntity entity)
        {
            var fecha = new FechaEvento(entity.FechaInicio);
            var duracion = new DuracionEvento(entity.DuracionHoras, entity.DuracionMinutos);
            var estado = new EstadoEvento(entity.Estado);

            var secciones = entity.Secciones.Select(s =>
                new Seccion(s.Id, s.Nombre, s.Capacidad, new PrecioEntrada(s.PrecioMonto))
            ).ToList();

            var evento = Evento.Crear(
                entity.Nombre,
                entity.Descripcion,
                fecha,
                duracion,
                entity.OrganizadorId,
                entity.VenueId,
                entity.Categoria,
                entity.TarifaPublicacion,
                secciones);

            evento.ClearDomainEvents();

            SetPrivateProperty(evento, nameof(Evento.Id), entity.Id);
            SetPrivateProperty(evento, nameof(Evento.Estado), estado);
            SetPrivateProperty(evento, nameof(Evento.TransaccionPagoId), entity.TransaccionPagoId);
            SetPrivateProperty(evento, nameof(Evento.Version), entity.Version);
            SetPrivateProperty(evento, nameof(Evento.FechaCreacion), entity.FechaCreacion);
            SetPrivateProperty(evento, nameof(Evento.FechaPublicacion), entity.FechaPublicacion);

            var principal = CrearMediaDesdeEntity(entity.ImagenPrincipalBlobName, entity.ImagenPrincipalContentType, entity.ImagenPrincipalSizeBytes, esImagen: true);
            var folleto = CrearMediaDesdeEntity(entity.FolletoBlobName, entity.FolletoContentType, entity.FolletoSizeBytes, esImagen: false);
            var secundarias = DeserializarSecundarias(entity.ImagenesSecundariasJson);
            evento.SincronizarMedia(principal, secundarias, folleto);

            return evento;
        }

        /// <summary>
        /// Mapea un agregado de dominio a una entidad de persistencia.
        /// </summary>
        private EventoEntity MapToEntity(Evento evento)
        {
            return new EventoEntity
            {
                Id = evento.Id,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                OrganizadorId = evento.OrganizadorId,
                VenueId = evento.VenueId,
                Categoria = evento.Categoria,
                TarifaPublicacion = evento.TarifaPublicacion,
                FechaInicio = evento.Fecha.Valor,
                DuracionHoras = evento.Duracion.Horas,
                DuracionMinutos = evento.Duracion.Minutos,
                Estado = evento.Estado.Valor,
                TransaccionPagoId = evento.TransaccionPagoId,
                Version = evento.Version,
                FechaCreacion = evento.FechaCreacion,
                FechaPublicacion = evento.FechaPublicacion,
                Secciones = evento.Secciones.Select(s => new SeccionEntity
                {
                    Id = s.Id,
                    EventoId = evento.Id,
                    Nombre = s.Nombre,
                    Capacidad = s.Capacidad,
                    PrecioMonto = s.Precio.Valor
                }).ToList(),
                ImagenPrincipalBlobName = evento.ImagenPrincipal?.BlobName,
                ImagenPrincipalContentType = evento.ImagenPrincipal?.ContentType,
                ImagenPrincipalSizeBytes = evento.ImagenPrincipal?.SizeBytes,
                ImagenesSecundariasJson = SerializarSecundarias(evento.ImagenesSecundarias),
                FolletoBlobName = evento.FolletoPdf?.BlobName,
                FolletoContentType = evento.FolletoPdf?.ContentType,
                FolletoSizeBytes = evento.FolletoPdf?.SizeBytes
            };
        }

        /// <summary>
        /// Establece una propiedad privada usando reflexión.
        /// </summary>
        private void SetPrivateProperty(object obj, string propertyName, object? value)
        {
            var property = obj.GetType().GetProperty(propertyName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            property?.SetValue(obj, value);
        }

        private static MediaAsset? CrearMediaDesdeEntity(string? blobName, string? contentType, long? sizeBytes, bool esImagen)
        {
            if (string.IsNullOrWhiteSpace(blobName) || string.IsNullOrWhiteSpace(contentType) || sizeBytes is null)
            {
                return null;
            }

            return esImagen
                ? MediaAsset.CrearImagen(blobName, contentType, sizeBytes.Value)
                : MediaAsset.CrearPdf(blobName, contentType, sizeBytes.Value);
        }

        private static List<MediaAsset> DeserializarSecundarias(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<MediaAsset>();
            }

            try
            {
                var dtoList = JsonSerializer.Deserialize<List<MediaAssetDto>>(json);
                if (dtoList == null)
                {
                    return new List<MediaAsset>();
                }

                return dtoList
                    .Where(d => d != null && !string.IsNullOrWhiteSpace(d.BlobName) && !string.IsNullOrWhiteSpace(d.ContentType))
                    .Select(d => MediaAsset.CrearImagen(d.BlobName!, d.ContentType!, d.SizeBytes))
                    .ToList();
            }
            catch
            {
                return new List<MediaAsset>();
            }
        }

        private static string SerializarSecundarias(IEnumerable<MediaAsset> assets)
        {
            var dto = assets.Select(a => new MediaAssetDto(a.BlobName, a.ContentType, a.SizeBytes)).ToList();
            return JsonSerializer.Serialize(dto);
        }

        private sealed record MediaAssetDto(string BlobName, string ContentType, long SizeBytes);
    }
}

