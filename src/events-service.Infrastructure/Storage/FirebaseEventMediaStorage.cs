using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Infrastructure.Storage;

public class FirebaseEventMediaStorage : IEventMediaStorage
{
    private readonly FirebaseStorageOptions _options;
    private readonly ILogger<FirebaseEventMediaStorage> _logger;
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public FirebaseEventMediaStorage(IOptions<FirebaseStorageOptions> options, ILogger<FirebaseEventMediaStorage> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_options.BucketName))
            throw new ArgumentException("BucketName is required in FirebaseStorageOptions.");

        _bucketName = _options.BucketName;
        
        GoogleCredential credential;
        
        // 1. Verificar si estamos usando el Emulador de Storage
        var emulatorHost = Environment.GetEnvironmentVariable("STORAGE_EMULATOR_HOST");
        if (!string.IsNullOrWhiteSpace(emulatorHost))
        {
            _logger.LogInformation("Using Firebase Storage Emulator at {Host}", emulatorHost);
            // El emulador acepta cualquier token, creamos uno dummy "owner"
            credential = GoogleCredential.FromAccessToken("owner");
        }
        else if (!string.IsNullOrWhiteSpace(_options.CredentialsPath) && File.Exists(_options.CredentialsPath))
        {
            credential = GoogleCredential.FromFile(_options.CredentialsPath);
        }
        else
        {
            // Fallback a credenciales por defecto
            _logger.LogWarning("No credentials path provided or file not found. Using Application Default Credentials.");
            credential = GoogleCredential.GetApplicationDefault();
        }

        // Crear el cliente de Storage directamente
        _storageClient = StorageClient.Create(credential);
    }

    public async Task<StoredBlob> UploadImageAsync(Guid eventoId, UploadFile file, bool esPrincipal, CancellationToken cancellationToken = default)
    {
        ValidateFile(file, isImage: true);

        var sanitizedFileName = SanitizeFileName(file.FileName);
        // Estructura: eventos/{eventoId}/principal/{guid}-{fileName} o eventos/{eventoId}/secundarias/{guid}-{fileName}
        var folder = esPrincipal ? "principal" : "secundarias";
        var blobName = $"eventos/{eventoId}/{folder}/{Guid.NewGuid()}-{sanitizedFileName}";

        return await UploadBlobAsync(blobName, file, cancellationToken);
    }

    public async Task<StoredBlob> UploadBrochureAsync(Guid eventoId, UploadFile file, CancellationToken cancellationToken = default)
    {
        ValidateFile(file, isImage: false);

        var sanitizedFileName = SanitizeFileName(file.FileName);
        // Estructura: eventos/{eventoId}/folleto/{guid}-{fileName}
        var blobName = $"eventos/{eventoId}/folleto/{Guid.NewGuid()}-{sanitizedFileName}";

        return await UploadBlobAsync(blobName, file, cancellationToken);
    }

    private void ValidateFile(UploadFile file, bool isImage)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        
        // Validar tamaño (regla de dominio re-verificada aquí como defensa en profundidad)
        if (file.Length > MediaAsset.MaxSizeBytes)
        {
            throw new ArgumentException($"El archivo excede el límite de {MediaAsset.MaxSizeBytes} bytes.", nameof(file));
        }

        // Validar Content-Type
        bool isValidType = isImage 
            ? Array.Exists(MediaAsset.AllowedImageContentTypes, x => x.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase))
            : Array.Exists(MediaAsset.AllowedPdfContentTypes, x => x.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase));

        if (!isValidType)
        {
            throw new ArgumentException($"Tipo de contenido no permitido: {file.ContentType}", nameof(file));
        }
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return "unnamed-file";
        
        // Eliminar extensión para procesar el nombre base
        var extension = Path.GetExtension(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        // Reemplazar caracteres no alfanuméricos con guiones por seguridad
        var sanitized = Regex.Replace(nameWithoutExt, "[^a-zA-Z0-9-]", "-");
        
        // Evitar múltiples guiones seguidos
        sanitized = Regex.Replace(sanitized, "-+", "-").Trim('-');

        if (string.IsNullOrWhiteSpace(sanitized)) sanitized = "file";

        return (sanitized + extension).ToLowerInvariant();
    }

    private async Task<StoredBlob> UploadBlobAsync(string blobName, UploadFile file, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream(file.Content);
            
            var obj = await _storageClient.UploadObjectAsync(
                _bucketName,
                blobName,
                file.ContentType,
                stream,
                cancellationToken: cancellationToken
            );

            // Generar URL. Si tenemos PublicBaseUrl configurada, la usamos (asumiendo acceso público configurado en el bucket).
            // Si no, generamos una Signed URL válida por un tiempo largo (e.g., 7 días) o usamos la MediaLink si es suficiente (requiere auth usualmente).
            // Para este caso, generaremos una Signed URL V4 si no hay PublicBaseUrl.
            
            string publicUrl;
            if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            {
                 // Construcción simple para buckets públicos
                 var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
                 // Hay que tener cuidado con el encoding de la URL
                 publicUrl = $"{baseUrl}/{System.Uri.EscapeDataString(blobName)}?alt=media";
            }
            else
            {
                // Generar Signed URL
                var urlSigner = UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());
                if (!string.IsNullOrWhiteSpace(_options.CredentialsPath))
                {
                    // Si especificamos ruta, intentamos cargarla para firmar. 
                    // El _storageClient ya tiene la credencial, pero UrlSigner necesita instanciarse tambien.
                    // Nota: ServiceAccountCredential es necesario para firmar.
                    if (File.Exists(_options.CredentialsPath)) 
                    {
                         using var streamCred = File.OpenRead(_options.CredentialsPath);
                         urlSigner = UrlSigner.FromCredential(ServiceAccountCredential.FromServiceAccountData(streamCred));
                    }
                }

                // Firmar por 7 días (máximo permitido para V4 es 7 días) o configurar según necesidad.
                // Aquí usamos 6 días para estar seguros.
                publicUrl = await urlSigner.SignAsync(
                    _bucketName,
                    blobName,
                    TimeSpan.FromDays(6),
                    HttpMethod.Get);
            }

            _logger.LogInformation("Archivo subido exitosamente a Firebase Storage: {BlobName}", blobName);

            return new StoredBlob(blobName, file.ContentType, (long)(obj.Size ?? 0), publicUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo a Firebase Storage: {BlobName}", blobName);
            // Aquí podríamos lanzar una excepción específica de dominio si fuera necesario
            throw new InvalidOperationException($"Error subiendo archivo a storage: {ex.Message}", ex);
        }
    }
}
