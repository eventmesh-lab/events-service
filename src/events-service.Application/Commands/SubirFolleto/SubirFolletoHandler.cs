using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Commands.SubirFolleto;

public record SubirFolletoCommand(Guid EventoId, string FileName, string ContentType, Stream Content, long Length) : IRequest<string>;

public class SubirFolletoHandler : IRequestHandler<SubirFolletoCommand, string>
{
    private readonly IEventoRepository _repository;
    private readonly IEventMediaStorage _storage;

    public SubirFolletoHandler(IEventoRepository repository, IEventMediaStorage storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<string> Handle(SubirFolletoCommand request, CancellationToken cancellationToken)
    {
        var evento = await _repository.GetByIdAsync(request.EventoId);
        if (evento == null)
        {
            throw new KeyNotFoundException($"No se encontró el evento con ID {request.EventoId}");
        }

        using var memoryStream = new MemoryStream();
        await request.Content.CopyToAsync(memoryStream, cancellationToken);
        var fileBytes = memoryStream.ToArray();

        var uploadFile = new UploadFile(request.FileName, request.ContentType, fileBytes, request.Length);

        // Subir a Storage
        var storedBlob = await _storage.UploadBrochureAsync(request.EventoId, uploadFile, cancellationToken);

        // Actualizar Dominio
        var mediaAsset = MediaAsset.CrearPdf(storedBlob.BlobName, storedBlob.ContentType, storedBlob.Length);
        evento.DefinirFolleto(mediaAsset);

        await _repository.UpdateAsync(evento);

        return storedBlob.Uri;
    }
}
