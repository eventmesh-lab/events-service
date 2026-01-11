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
    private static readonly HttpClient _httpClient = new HttpClient();

    private readonly FirebaseStorageOptions _options;
    private readonly ILogger<FirebaseEventMediaStorage> _logger;
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly string? _emulatorHost;

    public FirebaseEventMediaStorage(IOptions<FirebaseStorageOptions> options, ILogger<FirebaseEventMediaStorage> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_options.BucketName))
            throw new ArgumentException("BucketName is required in FirebaseStorageOptions.");

        _bucketName = _options.BucketName;
        
        // 1. Verificar si estamos usando el Emulador de Storage
        _emulatorHost = Environment.GetEnvironmentVariable("STORAGE_EMULATOR_HOST");
        if (!string.IsNullOrWhiteSpace(_emulatorHost))
        {
            _logger.LogInformation("Using Firebase Storage Emulator at {Host}", _emulatorHost);

            // Ajustar BaseUri: El cliente .NET espera que apunte a la raíz de la API JSON
            // Por defecto es https://storage.googleapis.com/storage/v1/
            // Si el emulador es http://host:9199, debemos añadir /storage/v1/
            var baseUri = _emulatorHost;
            if (!baseUri.EndsWith("/")) baseUri += "/";
            if (!baseUri.Contains("storage/v1")) baseUri += "storage/v1/";

            _logger.LogInformation("Adjusted BaseUri for Emulator: {BaseUri}", baseUri);
            
            // Usamos UnauthenticatedAccess = true para el emulador
            var builder = new StorageClientBuilder
            {
                BaseUri = baseUri,
                UnauthenticatedAccess = true 
            };

            _storageClient = builder.Build();
        }
        else
        {
            GoogleCredential credential;
            if (!string.IsNullOrWhiteSpace(_options.CredentialsPath) && File.Exists(_options.CredentialsPath))
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

    public async Task<string> GetFileUrlAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobName)) return string.Empty;

        // Si tenemos URL base pública (emulador o bucket público), construimos la URL directa
        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
             var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
             return $"{baseUrl}/{System.Uri.EscapeDataString(blobName)}?alt=media";
        }

        // Check for emulator host fallback
        var emulatorHost = Environment.GetEnvironmentVariable("STORAGE_EMULATOR_HOST");
        if (!string.IsNullOrWhiteSpace(emulatorHost))
        {
             var baseUrl = emulatorHost.TrimEnd('/');
             return $"{baseUrl}/v0/b/{_bucketName}/o/{System.Uri.EscapeDataString(blobName)}?alt=media";
        }

        // Si no, generamos Signed URL
        try 
        {
            var urlSigner = UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());
            
            if (!string.IsNullOrWhiteSpace(_options.CredentialsPath) && File.Exists(_options.CredentialsPath)) 
            {
                 using var streamCred = File.OpenRead(_options.CredentialsPath);
                 urlSigner = UrlSigner.FromCredential(ServiceAccountCredential.FromServiceAccountData(streamCred));
            }
            
            return await urlSigner.SignAsync(
                _bucketName,
                blobName,
                TimeSpan.FromDays(1), // URL válida por 1 día al solicitarla bajo demanda
                HttpMethod.Get);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando Signed URL para {BlobName}", blobName);
            return string.Empty; 
        }
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

    private async Task<StoredBlob> UploadToEmulatorAsync(string emulatorHost, string blobName, UploadFile file, CancellationToken cancellationToken)
    {
        var baseHost = emulatorHost.TrimEnd('/');
        
        // Normalizar host
        if (baseHost.EndsWith("storage/v1")) 
             baseHost = baseHost.Substring(0, baseHost.IndexOf("storage/v1"));
        baseHost = baseHost.TrimEnd('/');

        // 1. Asegurar Bucket (Intento, sin fallar si no funciona la API)
        try 
        {
            var projectId = !string.IsNullOrEmpty(_options.ProjectId) ? _options.ProjectId : "eventmesh-local";
            var createBucketUrl = $"{baseHost}/storage/v1/b?project={projectId}";
            var bucketJson = $"{{\"name\": \"{_bucketName}\"}}";
            using var bucketContent = new StringContent(bucketJson, System.Text.Encoding.UTF8, "application/json");
            var createResp = await _httpClient.PostAsync(createBucketUrl, bucketContent, cancellationToken);
             // Ignoramos resultado, muchos emuladores no implementan CREATE, pero asumen el bucket por defecto.
        }
        catch { /* Ignore */ }

        // 2. Subir usando Resumable Upload (Manual)
        // La carga simple (uploadType=media) a veces falla en el emulador (400 Bad Request).
        // La carga resumable es más robusta y es lo que la SDK intenta hacer, pero aquí controlamos la respuesta de error text/plain.
        try 
        {
            // Paso A: Iniciar Sesión de Subida
            var initiateUrl = $"{baseHost}/upload/storage/v1/b/{_bucketName}/o?uploadType=resumable&name={System.Uri.EscapeDataString(blobName)}";

            using var initRequest = new HttpRequestMessage(HttpMethod.Post, initiateUrl);
            initRequest.Headers.Add("X-Upload-Content-Type", file.ContentType);
            // Enviamos metadata básica vacía o mínima
            initRequest.Content = new StringContent($"{{\"contentType\": \"{file.ContentType}\"}}", System.Text.Encoding.UTF8, "application/json");

            var initResponse = await _httpClient.SendAsync(initRequest, cancellationToken);

            if (!initResponse.IsSuccessStatusCode)
            {
                var error = await initResponse.Content.ReadAsStringAsync();
                _logger.LogError("Emulator Resumable Init Failed: {StatusCode} {Error}", initResponse.StatusCode, error);
                throw new InvalidOperationException($"Emulator Init Failed: {initResponse.StatusCode} {error}");
            }

            var uploadUrl = initResponse.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(uploadUrl))
                throw new InvalidOperationException("Emulator did not return a Location header for resumable upload.");

            // Fix URL: El emulador a veces devuelve http://0.0.0.0:9199/...
            // Debemos reemplazar 0.0.0.0 por el host real del emulador si estamos en docker network.
            if (uploadUrl.Contains("://0.0.0.0:"))
            {
                 // Extraemos el host base de la config original
                 var uriBuilder = new UriBuilder(uploadUrl);
                 var originalHostUri = new Uri(baseHost);
                 uriBuilder.Host = originalHostUri.Host;
                 // Preservar puerto original del emulador si es distinto? 
                 // Normalmente baseHost ya tiene el puerto correcto.
                 uploadUrl = uriBuilder.ToString();
            }
            
            // Paso B: Subir Bytes (PUT)
            using var fileContent = new ByteArrayContent(file.Content);
            // NO establezcas Content-Type en el PUT de content si ya se definió en X-Upload-Content-Type, 
            // aunque GCS estándar lo ignora, el emulador puede ser estricto. 
            // Pero es buena práctica poner el Content-Length que HttpClient pone.

            var uploadResponse = await _httpClient.PutAsync(uploadUrl, fileContent, cancellationToken);

            if (!uploadResponse.IsSuccessStatusCode)
            {
                var error = await uploadResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Emulator PUT Failed: {uploadResponse.StatusCode} {error}");
            }

            // Exitoso
            string publicUrl = await GetFileUrlAsync(blobName, cancellationToken);
            return new StoredBlob(blobName, file.ContentType, file.Length, publicUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo a Firebase Storage (Emulator Resumable): {BlobName}", blobName);
            throw new InvalidOperationException($"Error subiendo archivo a emulador: {ex.Message}", ex);
        }
    }

    private async Task<StoredBlob> UploadBlobAsync(string blobName, UploadFile file, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_emulatorHost))
        {
             return await UploadToEmulatorAsync(_emulatorHost, blobName, file, cancellationToken);
        }

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
