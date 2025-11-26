using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using events_service.Domain.Entities;
using events_service.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace events_service.Infrastructure.Fallback
{
    /// <summary>
    /// Almacena eventos en un archivo JSON para escenarios de fallback.
    /// </summary>
    public class JsonEventoFallbackStore : IEventoFallbackStore
    {
        private readonly string _filePath;
        private readonly ILogger<JsonEventoFallbackStore> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true
        };

        public JsonEventoFallbackStore(IOptions<EventoFallbackOptions> options, ILogger<JsonEventoFallbackStore> logger)
        {
            var resolvedPath = options?.Value?.ResolvePath();
            _filePath = string.IsNullOrWhiteSpace(resolvedPath)
                ? Path.Combine(AppContext.BaseDirectory, "App_Data", "events-fallback.json")
                : resolvedPath!;
            _logger = logger;
        }

        public async Task SaveAsync(Evento evento, CancellationToken cancellationToken = default)
        {
            if (evento == null)
            {
                throw new ArgumentNullException(nameof(evento));
            }

            await _gate.WaitAsync(cancellationToken);
            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var items = await LoadAllInternalAsync(cancellationToken);
                var dto = EventoFallbackDto.FromDomain(evento);

                items.RemoveAll(x => x.Id == dto.Id);
                items.Add(dto);

                await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(stream, items, _serializerOptions, cancellationToken);
                _logger.LogInformation("Evento {EventoId} almacenado en fallback JSON", evento.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar evento {EventoId} en fallback JSON", evento.Id);
                throw;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<Evento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                var items = await LoadAllInternalAsync(cancellationToken);
                var dto = items.LastOrDefault(x => x.Id == id);
                return dto?.ToDomain();
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener evento {EventoId} desde fallback JSON", id);
                throw;
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task<List<EventoFallbackDto>> LoadAllInternalAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_filePath))
            {
                return new List<EventoFallbackDto>();
            }

            await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var items = await JsonSerializer.DeserializeAsync<List<EventoFallbackDto>>(stream, _serializerOptions, cancellationToken)
                        ?? new List<EventoFallbackDto>();
            return items;
        }

        private sealed class EventoFallbackDto
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public Guid OrganizadorId { get; set; }
            public Guid VenueId { get; set; }
            public string Categoria { get; set; } = string.Empty;
            public decimal TarifaPublicacion { get; set; }
            public DateTime Fecha { get; set; }
            public int DuracionHoras { get; set; }
            public int DuracionMinutos { get; set; }
            public string Estado { get; set; } = string.Empty;
            public Guid? TransaccionPagoId { get; set; }
            public DateTime FechaCreacion { get; set; }
            public DateTime? FechaPublicacion { get; set; }
            public int Version { get; set; }
            public List<SeccionFallbackDto> Secciones { get; set; } = new();

            public static EventoFallbackDto FromDomain(Evento evento)
            {
                return new EventoFallbackDto
                {
                    Id = evento.Id,
                    Nombre = evento.Nombre,
                    Descripcion = evento.Descripcion,
                    OrganizadorId = evento.OrganizadorId,
                    VenueId = evento.VenueId,
                    Categoria = evento.Categoria,
                    TarifaPublicacion = evento.TarifaPublicacion,
                    Fecha = evento.Fecha.Valor,
                    DuracionHoras = evento.Duracion.Horas,
                    DuracionMinutos = evento.Duracion.Minutos,
                    Estado = evento.Estado.Valor,
                    TransaccionPagoId = evento.TransaccionPagoId,
                    FechaCreacion = evento.FechaCreacion,
                    FechaPublicacion = evento.FechaPublicacion,
                    Version = evento.Version,
                    Secciones = evento.Secciones.Select(s => new SeccionFallbackDto
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Capacidad = s.Capacidad,
                        Precio = s.Precio.Valor
                    }).ToList()
                };
            }

            public Evento ToDomain()
            {
                var fecha = new FechaEvento(Fecha);
                var duracion = new DuracionEvento(DuracionHoras, DuracionMinutos);
                var secciones = Secciones.Select(s => s.ToDomain()).ToList();
                var evento = Evento.Crear(Nombre, Descripcion, fecha, duracion, OrganizadorId, VenueId, Categoria, TarifaPublicacion, secciones);
                evento.ClearDomainEvents();
                SetProperty(evento, nameof(Evento.Id), Id);
                SetProperty(evento, nameof(Evento.Estado), new EstadoEvento(Estado));
                SetProperty(evento, nameof(Evento.TransaccionPagoId), TransaccionPagoId);
                SetProperty(evento, nameof(Evento.Version), Version);
                SetProperty(evento, nameof(Evento.FechaCreacion), FechaCreacion);
                SetProperty(evento, nameof(Evento.FechaPublicacion), FechaPublicacion);
                return evento;
            }

            private static void SetProperty(object target, string propertyName, object? value)
            {
                var property = target.GetType().GetProperty(propertyName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                property?.SetValue(target, value);
            }
        }

        private sealed class SeccionFallbackDto
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int Capacidad { get; set; }
            public decimal Precio { get; set; }

            public Seccion ToDomain()
            {
                return new Seccion(Id, Nombre, Capacidad, new PrecioEntrada(Precio));
            }
        }
    }
}
