using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using events_service.Domain.Ports;

namespace events_service.Application.Commands.CancelarEvento;

public record CancelarEventoCommand(Guid EventoId, string Motivo, string CanceladoPor) : IRequest;

public class CancelarEventoCommandHandler : IRequestHandler<CancelarEventoCommand>
{
    private readonly IEventoRepository _repository;
    private readonly IRegistrationClient _registrationClient;

    public CancelarEventoCommandHandler(IEventoRepository repository, IRegistrationClient registrationClient)
    {
        _repository = repository;
        _registrationClient = registrationClient;
    }

    public async Task Handle(CancelarEventoCommand request, CancellationToken ct)
    {
        var evento = await _repository.GetByIdAsync(request.EventoId, ct);
        if (evento == null) throw new KeyNotFoundException($"Evento {request.EventoId} no encontrado.");

        var count = await _registrationClient.GetRegistrationCountAsync(request.EventoId, ct);
        if (count == 0)
        {
            throw new InvalidOperationException("El evento no tiene inscripciones. Debe ser eliminado físicamente en su lugar.");
        }

        evento.Cancelar(request.Motivo, request.CanceladoPor, DateTime.UtcNow);
        await _repository.UpdateAsync(evento, ct);
    }
}
