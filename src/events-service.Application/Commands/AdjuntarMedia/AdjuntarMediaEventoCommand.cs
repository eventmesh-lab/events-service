using System;
using System.Collections.Generic;
using MediatR;

namespace events_service.Application.Commands.AdjuntarMedia
{
    /// <summary>
    /// Comando para adjuntar la imagen principal, imágenes secundarias y/o folleto PDF a un evento.
    /// </summary>
    public sealed record AdjuntarMediaEventoCommand : IRequest<AdjuntarMediaEventoResponse>
    {
        public Guid EventoId { get; init; }

        public UploadFileDto? ImagenPrincipal { get; set; }

        public List<UploadFileDto> ImagenesSecundarias { get; init; } = new();

        public UploadFileDto? FolletoPdf { get; set; }

        public sealed record UploadFileDto(string FileName, string ContentType, byte[] Content)
        {
            public long Length => Content?.LongLength ?? 0;
        }
    }

    /// <summary>
    /// Respuesta con los blobs almacenados.
    /// </summary>
    public sealed record AdjuntarMediaEventoResponse
    {
        public string? ImagenPrincipalUrl { get; init; }
        public List<string> ImagenesSecundariasUrls { get; init; } = new();
        public string? FolletoUrl { get; init; }
    }
}
