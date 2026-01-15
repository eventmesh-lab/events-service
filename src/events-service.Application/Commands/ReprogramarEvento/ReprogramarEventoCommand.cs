using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Commands.ReprogramarEvento;

public record ReprogramarEventoCommand(
    Guid EventoId, 
    DateTime NuevaFecha, 
    int NuevasHoras, 
    int NuevosMinutos, 
    string ReprogramadoPor) : IRequest;

public class ReprogramarEventoCommandHandler : IRequestHandler<ReprogramarEventoCommand>
{
    private readonly IEventoRepository _repository;

    public ReprogramarEventoCommandHandler(IEventoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ReprogramarEventoCommand request, CancellationToken ct)
    {
        var evento = await _repository.GetByIdAsync(request.EventoId, ct);
        if (evento == null) throw new KeyNotFoundException($"Evento {request.EventoId} no encontrado.");

        var nuevaFecha = new FechaEvento(request.NuevaFecha);
        var nuevaDuracion = new DuracionEvento(request.NuevasHoras, request.NuevosMinutos);

        evento.Reprogramar(nuevaFecha, nuevaDuracion, request.ReprogramadoPor, DateTime.UtcNow);
        await _repository.UpdateAsync(evento, ct);
    }
}
