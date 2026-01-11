using System;
using System.Threading;
using System.Threading.Tasks;

namespace events_service.Domain.Ports;

/// <summary>
/// Representa un archivo para ser subido al almacenamiento.
/// </summary>
/// <param name="FileName">Nombre original del archivo.</param>
/// <param name="ContentType">Tipo de contenido MIME.</param>
/// <param name="Content">Contenido en bytes.</param>
/// <param name="Length">Tamaño en bytes.</param>
public record UploadFile(string FileName, string ContentType, byte[] Content, long Length);

/// <summary>
/// Representa la información de un blob almacenado.
/// </summary>
/// <param name="BlobName">Ruta o nombre único del blob.</param>
/// <param name="ContentType">Tipo de contenido MIME.</param>
/// <param name="Length">Tamaño en bytes.</param>
/// <param name="Uri">URI de acceso (pública o firmada).</param>
public record StoredBlob(string BlobName, string ContentType, long Length, string Uri);

/// <summary>
/// Puerto para el servicio de almacenamiento de medios de eventos.
/// </summary>
public interface IEventMediaStorage
{
    /// <summary>
    /// Sube una imagen asociada a un evento.
    /// </summary>
    Task<StoredBlob> UploadImageAsync(Guid eventoId, UploadFile file, bool esPrincipal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sube un folleto PDF asociado a un evento.
    /// </summary>
    Task<StoredBlob> UploadBrochureAsync(Guid eventoId, UploadFile file, CancellationToken cancellationToken = default);
}
