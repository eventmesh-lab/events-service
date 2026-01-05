using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using events_service.Domain.Ports;
using Microsoft.Extensions.Options;

namespace events_service.Infrastructure.Storage
{
    /// <summary>
    /// Implementación de almacenamiento de blobs para eventos usando Azurite/Blob Storage.
    /// </summary>
    public sealed class EventMediaStorage : IEventMediaStorage
    {
        private readonly BlobContainerClient _containerClient;
        private readonly string _publicBaseUrl;

        public EventMediaStorage(IOptions<BlobStorageOptions> options)
        {
            var settings = options.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ArgumentException("Se requiere la cadena de conexión de BlobStorage.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(settings.Container))
            {
                throw new ArgumentException("Se requiere el nombre del contenedor de BlobStorage.", nameof(options));
            }

            var serviceClient = new BlobServiceClient(settings.ConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(settings.Container);
            _containerClient.CreateIfNotExists();

            _publicBaseUrl = string.IsNullOrWhiteSpace(settings.PublicBaseUrl)
                ? _containerClient.Uri.ToString().TrimEnd('/')
                : settings.PublicBaseUrl.TrimEnd('/');
        }

        public Task<StoredBlob> UploadImageAsync(Guid eventoId, UploadFile file, bool esPrincipal, CancellationToken cancellationToken = default)
        {
            var prefix = esPrincipal ? "principal" : "secundarias";
            var blobName = $"eventos/{eventoId}/{prefix}/{Guid.NewGuid()}-{Sanitize(file.FileName)}";
            return UploadAsync(blobName, file, cancellationToken);
        }

        public Task<StoredBlob> UploadBrochureAsync(Guid eventoId, UploadFile file, CancellationToken cancellationToken = default)
        {
            var blobName = $"eventos/{eventoId}/folleto/{Guid.NewGuid()}-{Sanitize(file.FileName)}";
            return UploadAsync(blobName, file, cancellationToken);
        }

        private async Task<StoredBlob> UploadAsync(string blobName, UploadFile file, CancellationToken cancellationToken)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(file.Content);
            var headers = new BlobHttpHeaders { ContentType = file.ContentType };
            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);

            var uri = _publicBaseUrl.EndsWith('/') ? $"{_publicBaseUrl}{blobName}" : $"{_publicBaseUrl}/{blobName}";
            return new StoredBlob(blobName, file.ContentType, file.Length, uri);
        }

        private static string Sanitize(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "file";
            }

            return fileName.Replace("..", string.Empty).Replace(" ", "-");
        }
    }
}
