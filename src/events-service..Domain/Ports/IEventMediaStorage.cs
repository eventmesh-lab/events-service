using System;
using System.Threading;
using System.Threading.Tasks;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Ports
{
    /// <summary>
    /// Puerto para almacenar blobs asociados a eventos (imágenes y folleto).
    /// </summary>
    public interface IEventMediaStorage
    {
        Task<StoredBlob> UploadImageAsync(Guid eventoId, UploadFile file, bool esPrincipal, CancellationToken cancellationToken = default);

        Task<StoredBlob> UploadBrochureAsync(Guid eventoId, UploadFile file, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Representa un archivo entrante para subir a blob storage.
    /// </summary>
    public sealed record UploadFile(string FileName, string ContentType, byte[] Content)
    {
        public long Length => Content?.LongLength ?? 0;
    }

    /// <summary>
    /// Resultado de un upload al blob storage.
    /// </summary>
    public sealed record StoredBlob(string BlobName, string ContentType, long SizeBytes, string Uri);
}
