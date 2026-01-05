namespace events_service.Infrastructure.Storage
{
    /// <summary>
    /// Configuración del blob storage (Azurite/local).
    /// </summary>
    public sealed class BlobStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Container { get; set; } = "eventos-media";
        public string? PublicBaseUrl { get; set; }
    }
}
