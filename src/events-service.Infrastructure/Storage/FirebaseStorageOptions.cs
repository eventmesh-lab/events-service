namespace events_service.Infrastructure.Storage;

public class FirebaseStorageOptions
{
    public string ProjectId { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string? CredentialsPath { get; set; }
    public string? PublicBaseUrl { get; set; }
}
