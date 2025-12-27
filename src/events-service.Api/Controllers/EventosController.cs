using MediatR;
using Microsoft.AspNetCore.Mvc;
using events_service.Application.Commands.CrearEvento;
using events_service.Application.Commands.EditarEvento;
using events_service.Application.Commands.FinalizarEvento;
using events_service.Application.Commands.IniciarEvento;
using events_service.Application.Commands.PagarPublicacion;
using events_service.Application.Commands.PublicarEvento;
using events_service.Api.DTOs;
using events_service.Domain.Ports;

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

    /// <summary>
    /// Inicializa una nueva instancia del controlador de eventos.
    /// </summary>
    public EventosController(IMediator mediator, IEventoRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
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
    /// Obtiene todos los eventos publicados.
    /// </summary>
    /// <returns>Lista de eventos publicados.</returns>
    /// <response code="200">Lista de eventos publicados obtenida exitosamente.</response>
    [HttpGet("publicados")]
    [ProducesResponseType(typeof(List<EventoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEventosPublicados()
    {
        var eventos = await _repository.GetPublicadosAsync();
        var dtos = eventos.Select(EventoResponseDto.FromDomain).ToList();
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
        var dtos = eventos.Select(EventoResponseDto.FromDomain).ToList();
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
        var dtos = eventos.Select(EventoResponseDto.FromDomain).ToList();
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

        var dto = EventoResponseDto.FromDomain(evento);
        return Ok(dto);
    }
}
