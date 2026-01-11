using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Commands.SubirImagenSecundaria;

public record SubirImagenSecundariaCommand(Guid EventoId, string FileName, string ContentType, Stream Content, long Length) : IRequest<string>;

public class SubirImagenSecundariaHandler : IRequestHandler<SubirImagenSecundariaCommand, string>
{
    private readonly IEventoRepository _repository;
    private readonly IEventMediaStorage _storage;

    public SubirImagenSecundariaHandler(IEventoRepository repository, IEventMediaStorage storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<string> Handle(SubirImagenSecundariaCommand request, CancellationToken cancellationToken)
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
        var storedBlob = await _storage.UploadImageAsync(request.EventoId, uploadFile, esPrincipal: false, cancellationToken);

        // Crear Asset
        var nuevoAsset = MediaAsset.CrearImagen(storedBlob.BlobName, storedBlob.ContentType, storedBlob.Length);

        // Agregar a la lista existente
        var listaActual = evento.ImagenesSecundarias.ToList();
        listaActual.Add(nuevoAsset);

        // Actualizar Dominio (esto validará el máximo de 5)
        evento.DefinirImagenesSecundarias(listaActual);

        await _repository.UpdateAsync(evento);

        return storedBlob.Uri;
    }
}
