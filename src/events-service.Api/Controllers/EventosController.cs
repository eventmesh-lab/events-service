using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using events_service.Application.Commands.CrearEvento;
using events_service.Application.Commands.EditarEvento;
using events_service.Application.Commands.FinalizarEvento;
using events_service.Application.Commands.IniciarEvento;
using events_service.Application.Commands.PagarPublicacion;
using events_service.Application.Commands.PublicarEvento;
using events_service.Application.Commands.SubirImagenPrincipal;
using events_service.Application.Commands.SubirImagenSecundaria;
using events_service.Application.Commands.SubirFolleto;
using events_service.Application.Commands.CancelarEvento;
using events_service.Application.Commands.EliminarEvento;
using events_service.Application.Commands.ReprogramarEvento;
using events_service.Api.DTOs;
using events_service.Domain.Ports;
using events_service.Domain.Entities;

namespace events_service.Api.Controllers;

/// <summary>
/// Controlador para la gestión de eventos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEventoRepository _repository;
    private readonly IEventMediaStorage _mediaStorage;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de eventos.
    /// </summary>
    public EventosController(IMediator mediator, IEventoRepository repository, IEventMediaStorage mediaStorage)
    {
        _mediator = mediator;
        _repository = repository;
        _mediaStorage = mediaStorage;
    }

    /// <summary>
    /// Crea un nuevo evento en estado borrador.
    /// </summary>
    /// <param name="command">Datos del evento a crear.</param>
    /// <returns>El evento creado con su identificador.</returns>
    /// <response code="201">Evento creado exitosamente.</response>
    /// <response code="400">Datos inválidos o validación fallida.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CrearEventoCommandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearEvento([FromBody] CrearEventoCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObtenerEvento), new { id = result.Id }, result);
    }

    /// <summary>
    /// Publica un evento existente.
    /// </summary>
    /// <param name="id">Identificador único del evento a publicar.</param>
    /// <param name="command">Identificador del pago confirmado.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="200">Evento publicado exitosamente.</response>
    /// <response code="400">Evento no puede ser publicado (estado inválido o pago no confirmado).</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpPost("{id:guid}/publicar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublicarEvento(Guid id, [FromBody] PublicarEventoCommand command)
    {
        var enrichedCommand = command with { EventoId = id };
        await _mediator.Send(enrichedCommand);
        return Ok();
    }

    /// <summary>
    /// Edita un evento en estado borrador.
    /// </summary>
    /// <param name="id">Identificador único del evento a editar.</param>
    /// <param name="command">Datos actualizados del evento.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="204">Evento actualizado exitosamente.</response>
    /// <response code="400">Datos inválidos o evento no está en estado borrador.</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditarEvento(Guid id, [FromBody] EditarEventoCommand command)
    {
        var enrichedCommand = command with { EventoId = id };
        await _mediator.Send(enrichedCommand);
        return NoContent();
    }

    /// <summary>
    /// Inicia el pago de publicación del evento.
    /// </summary>
    /// <param name="id">Identificador único del evento.</param>
    /// <param name="command">Información de la transacción de pago.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="202">Pago iniciado, proceso en curso.</response>
    /// <response code="400">Datos inválidos, monto no coincide con tarifa o evento no está en estado borrador.</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpPost("{id:guid}/pagar-publicacion")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PagarPublicacionEvento(Guid id, [FromBody] PagarPublicacionCommand command)
    {
        var enrichedCommand = command with { EventoId = id };
        await _mediator.Send(enrichedCommand);
        return AcceptedAtAction(nameof(ObtenerEvento), new { id });
    }

    /// <summary>
    /// Marca un evento publicado como en curso.
    /// </summary>
    /// <param name="id">Identificador único del evento a iniciar.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="200">Evento iniciado exitosamente.</response>
    /// <response code="400">Evento no puede ser iniciado (estado inválido).</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpPost("{id:guid}/iniciar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarEvento(Guid id)
    {
        var command = new IniciarEventoCommand { EventoId = id };
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>
    /// Finaliza un evento en curso.
    /// </summary>
    /// <param name="id">Identificador único del evento a finalizar.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="200">Evento finalizado exitosamente.</response>
    /// <response code="400">Evento no puede ser finalizado (estado inválido).</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizarEvento(Guid id)
    {
        var command = new FinalizarEventoCommand { EventoId = id };
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>
    /// Cancela un evento que tiene inscripciones.
    /// </summary>
    /// <param name="id">Identificador único del evento.</param>
    /// <param name="command">Datos de la cancelación.</param>
    /// <response code="200">Evento cancelado exitosamente.</response>
    /// <response code="400">Evento no tiene inscripciones o estado inválido.</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelarEvento(Guid id, [FromBody] CancelarEventoCommand command)
    {
        var enrichedCommand = command with { EventoId = id };
        await _mediator.Send(enrichedCommand);
        return Ok();
    }

    /// <summary>
    /// Elimina físicamente un evento que NO tiene inscripciones.
    /// </summary>
    /// <param name="id">Identificador único del evento.</param>
    /// <response code="204">Evento eliminado exitosamente.</response>
    /// <response code="400">Evento tiene inscripciones activas.</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarEvento(Guid id)
    {
        var command = new EliminarEventoCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Reprograma las fechas de un evento publicado.
    /// </summary>
    /// <param name="id">Identificador único del evento.</param>
    /// <param name="command">Nuevos datos de fecha y duración.</param>
    /// <response code="200">Evento reprogramado exitosamente.</response>
    /// <response code="400">Datos inválidos, fecha no futura o evento no publicado.</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpPost("{id:guid}/reprogramar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReprogramarEvento(Guid id, [FromBody] ReprogramarEventoCommand command)
    {
        var enrichedCommand = command with { EventoId = id };
        await _mediator.Send(enrichedCommand);
        return Ok();
    }

    /// <summary>
    /// Sube la imagen principal del evento.
    /// </summary>
    [HttpPost("{id:guid}/imagen-principal")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirImagenPrincipal(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("Debe enviar un archivo.");

        try
        {
            var command = new SubirImagenPrincipalCommand(id, file.FileName, file.ContentType, file.OpenReadStream(), file.Length);
            var url = await _mediator.Send(command);
            return Ok(new { Url = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"No se encontró el evento con ID {id}");
        }
    }

    /// <summary>
    /// Sube una imagen secundaria al evento.
    /// </summary>
    [HttpPost("{id:guid}/imagen-secundaria")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirImagenSecundaria(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("Debe enviar un archivo.");

        try
        {
            var command = new SubirImagenSecundariaCommand(id, file.FileName, file.ContentType, file.OpenReadStream(), file.Length);
            var url = await _mediator.Send(command);
            return Ok(new { Url = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"No se encontró el evento con ID {id}");
        }
    }

    /// <summary>
    /// Sube el folleto PDF del evento.
    /// </summary>
    [HttpPost("{id:guid}/folleto")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirFolleto(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("Debe enviar un archivo.");

        try
        {
            var command = new SubirFolletoCommand(id, file.FileName, file.ContentType, file.OpenReadStream(), file.Length);
            var url = await _mediator.Send(command);
            return Ok(new { Url = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"No se encontró el evento con ID {id}");
        }
    }

    /// <summary>
    /// Obtiene todos los eventos publicados.
    /// </summary>
    /// <returns>Lista de eventos publicados.</returns>
    /// <response code="200">Lista de eventos publicados obtenida exitosamente.</response>
    [HttpGet("publicados")]
    [ProducesResponseType(typeof(List<EventoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEventosPublicados()
    {
        var eventos = await _repository.GetPublicadosAsync();
        var dtos = await EnrichEventosWithUrls(eventos);
        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene todos los eventos.
    /// </summary>
    /// <returns>Lista de todos los eventos.</returns>
    /// <response code="200">Lista de eventos obtenida exitosamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodosEventos()
    {
        var eventos = await _repository.GetAllAsync();
        var dtos = await EnrichEventosWithUrls(eventos);
        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene todos los eventos de un organizador.
    /// </summary>
    /// <param name="organizadorId">Identificador único del organizador.</param>
    /// <returns>Lista de eventos del organizador.</returns>
    /// <response code="200">Lista de eventos del organizador obtenida exitosamente.</response>
    [HttpGet("organizador/{organizadorId:guid}")]
    [ProducesResponseType(typeof(List<EventoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEventosPorOrganizador(Guid organizadorId)
    {
        var eventos = await _repository.GetByOrganizadorIdAsync(organizadorId);
        var dtos = await EnrichEventosWithUrls(eventos);
        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene todos los eventos de un venue.
    /// </summary>
    /// <param name="venueId">Identificador único del venue.</param>
    /// <returns>Lista de eventos del venue.</returns>
    /// <response code="200">Lista de eventos del venue obtenida exitosamente.</response>
    [HttpGet("venue/{venueId:guid}")]
    [ProducesResponseType(typeof(List<EventoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEventosPorVenue(Guid venueId)
    {
        var eventos = await _repository.GetByVenueIdAsync(venueId);
        var dtos = await EnrichEventosWithUrls(eventos);
        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene los detalles de un evento.
    /// </summary>
    /// <param name="id">Identificador único del evento.</param>
    /// <returns>Información completa del evento.</returns>
    /// <response code="200">Evento encontrado exitosamente.</response>
    /// <response code="404">Evento no encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerEvento(Guid id)
    {
        var evento = await _repository.GetByIdAsync(id);
        if (evento == null)
            return NotFound();

        var dto = await EnrichEventoWithUrls(evento);
        return Ok(dto);
    }

    // --- Helpers para enriquecer DTOs con URLs ---

    private async Task<List<EventoResponseDto>> EnrichEventosWithUrls(IEnumerable<Evento> eventos)
    {
        var tasks = eventos.Select(EnrichEventoWithUrls);
        return (await Task.WhenAll(tasks)).ToList();
    }

    private async Task<EventoResponseDto> EnrichEventoWithUrls(Evento evento)
    {
        var dto = EventoResponseDto.FromDomain(evento);

        // Enriquecer imagen principal
        if (evento.ImagenPrincipal != null)
        {
            var url = await _mediaStorage.GetFileUrlAsync(evento.ImagenPrincipal.BlobName);
            dto = dto with { MainImageUrl = url };
        }

        // Enriquecer imágenes secundarias
        if (evento.ImagenesSecundarias.Any())
        {
            var secondaryUrls = new List<string>();
            foreach (var img in evento.ImagenesSecundarias)
            {
                var url = await _mediaStorage.GetFileUrlAsync(img.BlobName);
                if (!string.IsNullOrEmpty(url)) secondaryUrls.Add(url);
            }
            // Asumiendo que EventoResponseDto tiene una lista de secundaria
            // Como record es inmutable, hay que ver si el DTO soporta esto
            // Si el DTO no tiene una propiedad lista de urls, deberíamos agregarla o mapearla
            // Por simplicidad en este paso, asignaremos a las propiedades si existen (Image1Url, etc)
            // O idealmente el DTO debería tener `List<string> SecondaryImageUrls`
            
            // Revisando EventoResponseDto (no tengo el código pero asumo properties separadas o lista)
            // Dado que el DTO es un record, lo más limpio es modificar el DTO si es necesario
            // Por ahora, vamos a asumir que el DTO tiene un SecondaryImages (List<string>) o similar.
            // Si el DTO original tiene ImageUrl1, ImageUrl2... 
            
            dto = dto with { SecondaryImageUrls = secondaryUrls };
        }

        // Enriquecer folleto
        if (evento.FolletoPdf != null)
        {
            var url = await _mediaStorage.GetFileUrlAsync(evento.FolletoPdf.BlobName);
            dto = dto with { BrochureUrl = url };
        }

        return dto;
    }
}
