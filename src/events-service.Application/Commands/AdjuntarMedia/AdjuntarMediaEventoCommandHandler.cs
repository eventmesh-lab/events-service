using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using events_service.Domain.Entities;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Commands.AdjuntarMedia
{
    /// <summary>
    /// Maneja la asociación de blobs (imágenes y folleto) a un evento.
    /// </summary>
    public sealed class AdjuntarMediaEventoCommandHandler : IRequestHandler<AdjuntarMediaEventoCommand, AdjuntarMediaEventoResponse>
    {
        private const long MaxBytes = MediaAsset.MaxSizeBytes;
        private readonly IEventoRepository _eventoRepository;
        private readonly IEventMediaStorage _mediaStorage;

        public AdjuntarMediaEventoCommandHandler(IEventoRepository eventoRepository, IEventMediaStorage mediaStorage)
        {
            _eventoRepository = eventoRepository ?? throw new ArgumentNullException(nameof(eventoRepository));
            _mediaStorage = mediaStorage ?? throw new ArgumentNullException(nameof(mediaStorage));
        }

        public async Task<AdjuntarMediaEventoResponse> Handle(AdjuntarMediaEventoCommand request, CancellationToken cancellationToken)
        {
            var evento = await _eventoRepository.GetByIdAsync(request.EventoId, cancellationToken);
            if (evento == null)
            {
                throw new InvalidOperationException($"Evento {request.EventoId} no encontrado.");
            }

            string? principalUrl = null;
            var secundariasUrls = new List<string>();
            string? folletoUrl = null;

            if (request.ImagenPrincipal != null)
            {
                ValidarImagen(request.ImagenPrincipal);
                var stored = await _mediaStorage.UploadImageAsync(evento.Id, ToUpload(request.ImagenPrincipal), true, cancellationToken);
                var asset = MediaAsset.CrearImagen(stored.BlobName, stored.ContentType, stored.SizeBytes);
                evento.DefinirImagenPrincipal(asset);
                principalUrl = stored.Uri;
            }

            if (request.ImagenesSecundarias.Any())
            {
                var secundarias = new List<MediaAsset>();
                foreach (var img in request.ImagenesSecundarias)
                {
                    ValidarImagen(img);
                    var stored = await _mediaStorage.UploadImageAsync(evento.Id, ToUpload(img), false, cancellationToken);
                    secundarias.Add(MediaAsset.CrearImagen(stored.BlobName, stored.ContentType, stored.SizeBytes));
                    secundariasUrls.Add(stored.Uri);
                }

                evento.DefinirImagenesSecundarias(secundarias);
            }

            if (request.FolletoPdf != null)
            {
                ValidarPdf(request.FolletoPdf);
                var stored = await _mediaStorage.UploadBrochureAsync(evento.Id, ToUpload(request.FolletoPdf), cancellationToken);
                var asset = MediaAsset.CrearPdf(stored.BlobName, stored.ContentType, stored.SizeBytes);
                evento.DefinirFolleto(asset);
                folletoUrl = stored.Uri;
            }

            await _eventoRepository.UpdateAsync(evento, cancellationToken);

            return new AdjuntarMediaEventoResponse
            {
                ImagenPrincipalUrl = principalUrl,
                ImagenesSecundariasUrls = secundariasUrls,
                FolletoUrl = folletoUrl
            };
        }

        private static void ValidarImagen(AdjuntarMediaEventoCommand.UploadFileDto file)
        {
            if (file.Length <= 0)
                throw new InvalidOperationException("La imagen está vacía.");

            if (file.Length > MaxBytes)
                throw new InvalidOperationException("La imagen supera 1MB.");

            if (!MediaAsset.AllowedImageContentTypes.Any(ct => ct.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Formato de imagen no soportado (use jpg, png o webp).");
        }

        private static void ValidarPdf(AdjuntarMediaEventoCommand.UploadFileDto file)
        {
            if (file.Length <= 0)
                throw new InvalidOperationException("El folleto está vacío.");

            if (file.Length > MaxBytes)
                throw new InvalidOperationException("El folleto supera 1MB.");

            if (!MediaAsset.AllowedPdfContentTypes.Any(ct => ct.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("El folleto debe ser PDF.");
        }

        private static UploadFile ToUpload(AdjuntarMediaEventoCommand.UploadFileDto dto) =>
            new(dto.FileName, dto.ContentType, dto.Content);
    }
}
