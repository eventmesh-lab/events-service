using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using events_service.Domain.Ports;

namespace events_service.Application.Commands.EliminarEvento;

public record EliminarEventoCommand(Guid EventoId) : IRequest;

public class EliminarEventoCommandHandler : IRequestHandler<EliminarEventoCommand>
{
    private readonly IEventoRepository _repository;
    private readonly IRegistrationClient _registrationClient;
    private readonly IEventMediaStorage _mediaStorage;

    public EliminarEventoCommandHandler(IEventoRepository repository, IRegistrationClient registrationClient, IEventMediaStorage mediaStorage)
    {
        _repository = repository;
        _registrationClient = registrationClient;
        _mediaStorage = mediaStorage;
    }

    public async Task Handle(EliminarEventoCommand request, CancellationToken ct)
    {
        var evento = await _repository.GetByIdAsync(request.EventoId, ct);
        if (evento == null) throw new KeyNotFoundException($"Evento {request.EventoId} no encontrado.");

        var count = await _registrationClient.GetRegistrationCountAsync(request.EventoId, ct);
        if (count > 0)
        {
            throw new InvalidOperationException("El evento tiene inscripciones activas y no puede ser eliminado físicamente. Debe cancelarlo.");
        }

        await _repository.DeleteAsync(evento, ct);
        await _mediaStorage.DeleteAllFilesAsync(evento.Id, ct);
    }
}
