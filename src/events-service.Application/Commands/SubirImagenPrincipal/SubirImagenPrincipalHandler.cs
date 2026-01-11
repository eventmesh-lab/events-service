using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Commands.SubirImagenPrincipal;

public record SubirImagenPrincipalCommand(Guid EventoId, string FileName, string ContentType, Stream Content, long Length) : IRequest<string>;

public class SubirImagenPrincipalHandler : IRequestHandler<SubirImagenPrincipalCommand, string>
{
    private readonly IEventoRepository _repository;
    private readonly IEventMediaStorage _storage;

    public SubirImagenPrincipalHandler(IEventoRepository repository, IEventMediaStorage storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<string> Handle(SubirImagenPrincipalCommand request, CancellationToken cancellationToken)
    {
        var evento = await _repository.GetByIdAsync(request.EventoId);
        if (evento == null)
        {
            throw new KeyNotFoundException($"No se encontró el evento con ID {request.EventoId}");
        }

        // Leer stream a bytes para el record UploadFile
        using var memoryStream = new MemoryStream();
        await request.Content.CopyToAsync(memoryStream, cancellationToken);
        var fileBytes = memoryStream.ToArray();

        var uploadFile = new UploadFile(request.FileName, request.ContentType, fileBytes, request.Length);

        // Subir a Storage
        var storedBlob = await _storage.UploadImageAsync(request.EventoId, uploadFile, esPrincipal: true, cancellationToken);

        // Actualizar Dominio
        var mediaAsset = MediaAsset.CrearImagen(storedBlob.BlobName, storedBlob.ContentType, storedBlob.Length);
        evento.DefinirImagenPrincipal(mediaAsset);

        // Persistir cambios
        await _repository.UpdateAsync(evento);

        return storedBlob.Uri;
    }
}
